using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL.Repositories.Implementation
{
    public class DeliveryManRepository : GenericRepository<DeliveryMan>, IDeliveryManRepository
    {
        public DeliveryManRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
