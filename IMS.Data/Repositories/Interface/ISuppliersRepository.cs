using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;
using System.Linq.Expressions;

namespace IMS.Data.Repositories.Interfaces
{
    public interface ISuppliersRepository:IGenericRepository<Supplier>
    {
        Task<List<Supplier>> GetAllSuppliersWithProducts();
        Task<Supplier> GetSupplierBy(Expression<Func<Supplier, bool>> predicate);
    }
}
