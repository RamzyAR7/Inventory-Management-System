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
    public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
