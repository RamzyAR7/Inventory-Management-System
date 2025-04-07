using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly InventoryDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }


        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            // Get key name dynamically
            var keyName = _context.Model.FindEntityType(typeof(T))?
                                       .FindPrimaryKey()?
                                       .Properties.FirstOrDefault()?.Name;

            if (keyName == null)
                throw new InvalidOperationException($"Cannot find primary key for {typeof(T).Name}");

            // Try converting ID to actual key type
            var propertyType = typeof(T).GetProperty(keyName)?.PropertyType;
            var convertedId = Convert.ChangeType(id, propertyType!);

            // Use FindAsync
            var entity = await _dbSet.FindAsync(convertedId);

            if (entity == null)
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with ID {id} not found.");

            // Include navigation properties manually
            var entry = _context.Entry(entity);
            foreach (var includeProperty in includeProperties)
            {
                await entry.Reference(includeProperty).LoadAsync();
            }

            return entity;
        }


        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includeProperties)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            IQueryable<T> query = _dbSet.AsNoTracking();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.Where(predicate).ToListAsync();
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
                query = query.Include(includeProperty);
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

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;
                var id = (int)entry.Property(key).CurrentValue;
                var existingEntity = await GetByIdAsync(id);
                if (existingEntity == null)
                    throw new KeyNotFoundException($"Entity with ID {id} not found.");

                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                _dbSet.Update(entity);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
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

