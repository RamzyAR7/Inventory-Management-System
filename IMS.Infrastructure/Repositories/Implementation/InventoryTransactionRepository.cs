using IMS.Infrastructure.Context;
using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;


namespace IMS.Infrastructure.Repositories.Implementation
{
    public class InventoryTransactionRepository: GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(InventoryDbContext context) : base(context)
        {
        }
    }

}
