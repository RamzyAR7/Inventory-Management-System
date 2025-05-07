using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface ISuppliersRepository:IGenericRepository<Supplier>
    {
        Task<Supplier> GetSupplierAndProductsBy(Expression<Func<Supplier, bool>> predicate);
    }
}
