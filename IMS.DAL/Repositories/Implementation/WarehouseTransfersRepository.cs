using IMS.DAL.Context;
using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;

namespace IMS.DAL.Repositories.Implementation
{
    public class WarehouseTransfersRepository: GenericRepository<WarehouseTransfers>, IWarehouseTransfersRepository
    {
        public WarehouseTransfersRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
