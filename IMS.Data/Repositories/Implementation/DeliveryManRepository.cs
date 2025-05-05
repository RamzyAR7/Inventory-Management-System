using IMS.Data.Context;
using IMS.Data.Entities;
using IMS.Data.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Data.Repositories.Implementation
{
    public class DeliveryManRepository : GenericRepository<DeliveryMan>, IDeliveryManRepository
    {
        public DeliveryManRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
