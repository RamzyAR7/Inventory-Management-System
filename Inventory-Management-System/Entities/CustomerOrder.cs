namespace Inventory_Management_System.Entities
{
    public class CustomerOrder
    {
        #region Properties
        // Foreign key to Customer
        public Guid CustomerID { get; set; }
        // Foreign key to Order
        public Guid OrderID { get; set; }
        // NAVIGATION PROPERTIES
        public Customer Customer { get; set; }
        public Order Order { get; set; }
        #endregion
    }
}
