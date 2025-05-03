namespace IMS.Data.Entities
{
    public class WarehouseStock
    {
        #region Properties
        // Foreign key to Warehouse
        public Guid WarehouseID { get; set; }
        // Foreign key to Product
        public Guid ProductID { get; set; }
        public int StockQuantity { get; set; } = 0;

        // Navigation properties
        public Warehouse Warehouse { get; set; }
        public Product Product { get; set; }
        #endregion
    }
}
