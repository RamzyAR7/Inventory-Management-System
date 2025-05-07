using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL.Repositories.Implementation
{
    public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(InventoryDbContext context) : base(context)
        {
        }

        public Expression<Func<T, bool>> CombinePredicates<T>(Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(predicate1, parameter),
                Expression.Invoke(predicate2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

    }
}
