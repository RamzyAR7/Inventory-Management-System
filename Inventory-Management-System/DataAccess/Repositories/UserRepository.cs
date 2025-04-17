using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(InventoryDbContext context) : base(context)
        {
        }
        public async Task<User> GetByUserNameAsync(string userName, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<IEnumerable<User>> FindManagerAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }
    }
}
