using IMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.Models
{
    public class ProductViewModel
    {
        public Guid ProductID { get; set; }
        public string DisplayText { get; set; }
    }
}
