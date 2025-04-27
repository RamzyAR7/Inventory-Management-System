using Inventory_Management_System.Entities;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface ISuppliersRepository:IGenericRepository<Supplier>
    {
        Task<List<Supplier>> GetAllSuppliersWithProducts();
        Task<Supplier> GetSupplierBy(Expression<Func<Supplier, bool>> predicate);
    }
}
