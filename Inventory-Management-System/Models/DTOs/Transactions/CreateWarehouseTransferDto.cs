using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.InventoryTransaction
{
    public class CreateWarehouseTransferDto
    {
        [Required(ErrorMessage = "Source warehouse is required.")]
        public Guid FromWarehouseId { get; set; }

        [Required(ErrorMessage = "Destination warehouse is required.")]
        public Guid ToWarehouseId { get; set; }

        [Required(ErrorMessage = "Source product is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Destination product is required.")]
        public Guid ToProductId { get; set; } // Added

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
    }
}
