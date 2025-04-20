namespace Inventory_Management_System.Entities
{

    public class Customer
    {
        public Guid CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
    }
}