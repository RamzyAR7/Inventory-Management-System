namespace Inventory_Management_System.Entities
{
    public class CustomerOrder
    {
        // foreign key to Customer
        public Guid CustomerID { get; set; }
        // foreign key to Order
        public Guid OrderID { get; set; }

        // NAVIGATION PROPERTIES
        public Customer Customer { get; set; }

        public Order Order { get; set; }
    }
}
