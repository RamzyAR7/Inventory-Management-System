using Inventory_Management_System.Entities;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IUserRepository:IGenericRepository<User>
    {
        Task<User> GetByUserNameAsync(string userName, params Expression<Func<User, object>>[] includes);
    }
}
