using IMS.Infrastructure.Context;
using IMS.Infrastructure.Repositories.Interface;
using IMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Infrastructure.Repositories.Implementation
{
    public class DeliveryManRepository : GenericRepository<DeliveryMan>, IDeliveryManRepository
    {
        public DeliveryManRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
