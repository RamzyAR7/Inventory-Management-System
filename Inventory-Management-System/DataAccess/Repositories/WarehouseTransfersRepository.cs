using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class WarehouseTransfersRepository: GenericRepository<WarehouseTransfers>, IWarehouseTransfersRepository
    {
        public WarehouseTransfersRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
