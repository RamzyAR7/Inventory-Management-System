using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class InventoryTransactionRepository: GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(InventoryDbContext context) : base(context)
        {
        }
    }

}
