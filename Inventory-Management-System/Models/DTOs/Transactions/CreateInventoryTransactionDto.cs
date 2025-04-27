using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.InventoryTransaction
{
    public class CreateInventoryTransactionDto
    {
        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
