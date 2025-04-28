using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(Guid warehouseId);
        Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<WarehouseTransfers>> GetAllTransfersAsync();
        Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId);
        Task CreateInTransactionAsync(CreateInventoryTransactionDto dto);
        Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto); // Updated
    }
}
