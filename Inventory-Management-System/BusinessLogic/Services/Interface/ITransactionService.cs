using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface ITransactionService
    {
        Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(Guid warehouseId);
        Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<WarehouseTransfers>> GetAllTransfersAsync();
        Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId);
        Task CreateInOrOutTransactionAsync(CreateInventoryTransactionDto dto);
        Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto);
    }
}
