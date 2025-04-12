using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.ViewModels
{
    public class OrderViewModel: IValidatableObject
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }

        public Guid CustomerID { get; set; }
        public IEnumerable<SelectListItem> Customers { get; set; }

        public Guid WarehouseID { get; set; }
        public IEnumerable<SelectListItem> Warehouses { get; set; }

        public string Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }

}
