using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;

namespace IMS.DAL.Repositories.Implementation
{
    public class WarehouseTransfersRepository: GenericRepository<WarehouseTransfers>, IWarehouseTransfersRepository
    {
        public WarehouseTransfersRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
