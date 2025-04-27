using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.InventoryTransaction
{
    public class CreateWarehouseTransferDto
    {
        [Required]
        public Guid FromWarehouseId { get; set; }

        [Required]
        public Guid ToWarehouseId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
