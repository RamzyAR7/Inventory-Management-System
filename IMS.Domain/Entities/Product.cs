﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Domain.Entities
{
    public class Product
    {
        #region Properties
        public Guid ProductID { get; set; }
        [Required, MaxLength(100)]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int RecoderLevel { get; set; }
        public bool IsActive { get; set; } = true;

        // Foreign key to Category
        public Guid CategoryID { get; set; }

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<SupplierProduct> Suppliers { get; set; }
        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
        public ICollection<WarehouseTransfers> FromWarehouseTransfers { get; set; }
        public ICollection<WarehouseTransfers> ToWarehouseTransfers { get; set; }

        #endregion
    }
}
