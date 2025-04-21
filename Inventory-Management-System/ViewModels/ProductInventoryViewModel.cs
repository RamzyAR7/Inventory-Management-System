namespace Inventory_Management_System.ViewModels
{
    public class ProductInventoryViewModel
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status => Stock > 0 ? "In Stock" : "Out of Stock";
        public DateTime LastUpdated { get; set; }
    }
}
