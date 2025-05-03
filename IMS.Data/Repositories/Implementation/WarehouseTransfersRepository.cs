using IMS.Data.Context;
using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;

namespace IMS.Data.Repositories.Implementation
{
    public class WarehouseTransfersRepository: GenericRepository<WarehouseTransfers>, IWarehouseTransfersRepository
    {
        public WarehouseTransfersRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
