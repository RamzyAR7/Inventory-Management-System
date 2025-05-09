using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Infrastructure.Repositories.Implementation
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(first.Body, second.Body),
                first.Parameters);
        }
    }
}
