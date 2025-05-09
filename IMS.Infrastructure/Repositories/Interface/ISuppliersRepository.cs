using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories.Interfaces
{
    public interface ISuppliersRepository:IGenericRepository<Supplier>
    {
        Task<Supplier> GetSupplierAndProductsBy(Expression<Func<Supplier, bool>> predicate);
    }
}
