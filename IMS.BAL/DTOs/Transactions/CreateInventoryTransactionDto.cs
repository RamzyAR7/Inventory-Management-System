using System.ComponentModel.DataAnnotations;

namespace IMS.BAL.DTOs.Transactions
{
    public class CreateInventoryTransactionDto
    {
        [Required]
        public Guid SupplierID { get; set; }
        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]

        public int Quantity { get; set; }
    }
}
