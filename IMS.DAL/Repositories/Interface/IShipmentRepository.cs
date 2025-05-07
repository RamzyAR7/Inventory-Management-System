using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.DAL.Repositories.Interface
{
    public interface IShipmentRepository : IGenericRepository<Shipment>
    {
        Expression<Func<T, bool>> CombinePredicates<T>(Expression<Func<T, bool>> predicate1, Expression<Func<T, bool>> predicate2);
    }
}
