using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository:IGenericRepository<User>
    {
        Task<User> GetByUserNameAsync(string userName, params Expression<Func<User, object>>[] includes);
        Task<IEnumerable<User>> FindManagerAsync(Expression<Func<User, bool>> predicate);
    }
}
