﻿using IMS.Application.DTOs.User;
using IMS.Domain.Entities;

namespace IMS.Application.DTOs.Warehouse
{
    public class WarehouseResDto
    {
        public Guid WarehouseID { get; set; }
        public string Address { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public Guid ManagerID { get; set; }
        public UserResDto? Manager { get; set; }

        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}
