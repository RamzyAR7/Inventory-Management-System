using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Supplier
{
    public class SupplierResDto
    {
        [Key]
        public Guid SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<SupplierProduct> Products { get; set; }
    }
}
