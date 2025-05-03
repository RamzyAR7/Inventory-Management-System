using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;
using System.Linq.Expressions;

namespace IMS.Data.Repositories.Interfaces
{
    public interface IUserRepository:IGenericRepository<User>
    {
        Task<User> GetByUserNameAsync(string userName, params Expression<Func<User, object>>[] includes);
        Task<IEnumerable<User>> FindManagerAsync(Expression<Func<User, bool>> predicate);
    }
}
