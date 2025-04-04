namespace Inventory_Management_System.Entities
{
    public class CustomerOrder
    {
        // foreign key to Customer
        public int CustomerID { get; set; }
        // foreign key to Order
        public int OrderID { get; set; }

        // NAVIGATION PROPERTIES
        public Customer Customer { get; set; }

        public Order Order { get; set; }
    }
}
