using IMS.Infrastructure.Context;
using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;

namespace IMS.Infrastructure.Repositories.Implementation
{
    public class WarehouseTransfersRepository: GenericRepository<WarehouseTransfers>, IWarehouseTransfersRepository
    {
        public WarehouseTransfersRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
