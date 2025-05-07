using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.SharedServices.Interface
{
    public interface IWhoIsUserLoginService
    {
        Task<string> GetCurrentUserId();
        Task<string> GetCurrentUserRole();
        Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId);
    }
}
