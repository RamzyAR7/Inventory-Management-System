using IMS.DAL.Context;
using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;


namespace IMS.DAL.Repositories.Implementation
{
    public class InventoryTransactionRepository: GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(InventoryDbContext context) : base(context)
        {
        }
    }

}
