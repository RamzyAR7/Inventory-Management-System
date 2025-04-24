namespace Inventory_Management_System.Models.DTOs.Order
{
    public class CustomerOrderResDto
    {
        public Guid CustomerOrderID { get; set; }
        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
