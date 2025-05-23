﻿namespace IMS.Domain.Entities
{
    public class SupplierProduct
    {
        #region Properties
        // Foreign key to Supplier
        public Guid SupplierID { get; set; }
        // Foreign key to Product
        public Guid ProductID { get; set; }

        // Navigation properties
        public Supplier Supplier { get; set; }
        public Product Product { get; set; }
        #endregion
    }
}
