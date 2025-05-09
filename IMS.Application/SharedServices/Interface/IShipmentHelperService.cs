using IMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.SharedServices.Interface
{
    public interface IShipmentHelperService
    {
        Task<Shipment> ValidateUserAccessShipmentAsync(Guid shipmentId, Expression<Func<Shipment, object>>[]? includes = null);
        Task<IEnumerable<DeliveryMan>> GetAllFreeDeliveryMen();
    }
}
