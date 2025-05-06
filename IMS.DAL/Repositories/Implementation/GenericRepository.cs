using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using IMS.DAL.Context;
using IMS.DAL.Repositories.Interfaces;

namespace IMS.DAL.Repositories.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly InventoryDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        //public IQueryable<T> GetQueryable()
        //{
        //    return _dbSet.AsQueryable();
        //}

        //public async Task<bool> ExistsAsync(Guid id)
        //{
        //    var keyName = _context.Model.FindEntityType(typeof(T))
        //                    ?.FindPrimaryKey()
        //                    ?.Properties
        //                    ?.FirstOrDefault()
        //                    ?.Name;

        //    var propertyType = typeof(T).GetProperty(keyName!)?.PropertyType;
        //    var convertedId = Convert.ChangeType(id, propertyType!);

        //    var entity = await _dbSet.FindAsync(convertedId);
        //    return entity != null;
        //}


        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                if (include != null)
                {
                    query = query.Include(include);
                }
            }
            var result = await query.ToListAsync();
            return result ?? Enumerable.Empty<T>();
        }

        public async Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
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

            return await GetByExpressionAsync(lambda, includes);
        }

        public async Task<T?> GetByCompositeKeyAsync(Guid key1, Guid key2, params Expression<Func<T, object>>[] includes)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType!.FindPrimaryKey();
            if (primaryKey!.Properties.Count != 2)
                throw new InvalidOperationException("This method is for entities with exactly two key properties.");

            var keyName1 = primaryKey.Properties[0].Name;
            var keyName2 = primaryKey.Properties[1].Name;

            var parameter = Expression.Parameter(typeof(T), "e");
            var property1 = Expression.Property(parameter, keyName1);
            var property2 = Expression.Property(parameter, keyName2);
            var keyValue1 = Expression.Constant(key1);
            var keyValue2 = Expression.Constant(key2);
            var equals1 = Expression.Equal(property1, keyValue1);
            var equals2 = Expression.Equal(property2, keyValue2);
            var andAlso = Expression.AndAlso(equals1, equals2);
            var lambda = Expression.Lambda<Func<T, bool>>(andAlso, parameter);

            return await GetByExpressionAsync(lambda, includes);
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
            return await _dbSet.FirstOrDefaultAsync(predicate);
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

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize,Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includeProperties)
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

            return (items ?? Enumerable.Empty<T>(), totalCount);
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

            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType!.FindPrimaryKey();
            var keyProperties = primaryKey!.Properties;

            var parameter = Expression.Parameter(typeof(T), "e");
            Expression predicateBody = null;
            var keyValues = new object[keyProperties.Count];

            for (int i = 0; i < keyProperties.Count; i++)
            {
                var keyName = keyProperties[i].Name;
                var keyValue = typeof(T).GetProperty(keyName)!.GetValue(entity);
                keyValues[i] = keyValue;

                var property = Expression.Property(parameter, keyName);
                var constant = Expression.Constant(keyValue);
                var equals = Expression.Equal(property, constant);

                predicateBody = predicateBody == null ? equals : Expression.AndAlso(predicateBody, equals);
            }

            var predicate = Expression.Lambda<Func<T, bool>>(predicateBody!, parameter);
            var existing = await _dbSet.FirstOrDefaultAsync(predicate);

            if (existing == null)
                throw new KeyNotFoundException($"Entity with keys {string.Join(", ", keyValues)} not found.");

            var entry = _context.Entry(existing);
            var values = _context.Entry(entity).CurrentValues;

            foreach (var prop in values.Properties)
            {
                if (entry.Metadata.FindNavigation(prop.Name) == null && !keyProperties.Any(kp => kp.Name == prop.Name))
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

        public async Task DeleteAsync(Guid key1, Guid key2)
        {
            var entity = await GetByCompositeKeyAsync(key1, key2);
            if (entity == null)
                throw new KeyNotFoundException($"Entity with keys {key1}, {key2} not found.");

            _dbSet.Remove(entity);
        }
    }
}
