using IMS.Data.Context;
using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;


namespace IMS.Data.Repositories.Implementation
{
    public class InventoryTransactionRepository: GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(InventoryDbContext context) : base(context)
        {
        }
    }

}
