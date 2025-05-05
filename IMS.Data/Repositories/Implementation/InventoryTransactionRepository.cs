using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;


namespace IMS.DAL.Repositories.Implementation
{
    public class InventoryTransactionRepository: GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(InventoryDbContext context) : base(context)
        {
        }
    }

}
