using Azure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System;
using IMS.Data.Entities;

namespace IMS.BAL.DTOs.Order.Responce
{
    public class OrderDetailResponseDto
    {
        public Guid OrderID { get; set; }
        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; }
        public Guid WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CreatedByUserID { get; set; }
        public string CreatedByUserName { get; set; }
        public List<OrderDetailResponseItem> OrderDetails { get; set; } = new List<OrderDetailResponseItem>();
    }

    public class OrderDetailResponseItem
    {
        public Guid OrderDetailID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
