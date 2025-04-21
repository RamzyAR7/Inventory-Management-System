using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public InventoryDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                if (include != null) // Skip null includes
                {
                    query = query.Include(include);
                }
            }
            return await query.ToListAsync();
        }


        public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                if (include != null)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        // overload on GetByIdAsync
        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var keyName = _context.Model.FindEntityType(typeof(T))!
                                       .FindPrimaryKey()!
                                       .Properties[0].Name;

            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, keyName);
            var idValue = Expression.Constant(id);
            var equals = Expression.Equal(property, idValue);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            return await GetByIdAsync(lambda, includes);
        }


        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                if (include != null)
                {
                    query = query.Include(include);
                }
            }
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await FirstOrDefaultAsync(predicate, Array.Empty<Expression<Func<T, object>>>());
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                if (include != null)
                {
                    query = query.Include(include);
                }
            }
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize,
            Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includeProperties)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var includeProperty in includeProperties)
            {
                if (includeProperty != null)
                {
                    query = query.Include(includeProperty);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        //public async Task UpdateAsync(T entity)
        //{
        //    if (entity == null)
        //        throw new ArgumentNullException(nameof(entity));

        //    var entry = _context.Entry(entity);
        //    if (entry.State == EntityState.Detached)
        //    {
        //        var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;
        //        var id = (Guid)entry.Property(key).CurrentValue;
        //        var existingEntity = await GetByIdAsync(id);
        //        if (existingEntity == null)
        //            throw new KeyNotFoundException($"Entity with ID {id} not found.");

        //        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        //    }
        //    else
        //    {
        //        _dbSet.Update(entity);
        //    }
        //}


        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;
            var id = (Guid)typeof(T).GetProperty(key).GetValue(entity);
            var existing = await GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            var entry = _context.Entry(existing);
            var values = _context.Entry(entity).CurrentValues;

            foreach (var prop in values.Properties)
            {
                // ignore Navigation Properties like CustomerOrders
                if (entry.Metadata.FindNavigation(prop.Name) == null)
                {
                    entry.CurrentValues[prop.Name] = values[prop.Name];
                }
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var keyName = _context.Model.FindEntityType(typeof(T))
                            ?.FindPrimaryKey()
                            ?.Properties
                            ?.FirstOrDefault()
                            ?.Name;

            var propertyType = typeof(T).GetProperty(keyName!)?.PropertyType;
            var convertedId = Convert.ChangeType(id, propertyType!);

            var entity = await _dbSet.FindAsync(convertedId);
            return entity != null;
        }

       
    }
}
