using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(Guid warehouseId);
        Task<(IEnumerable<InventoryTransaction> Items, int TotalCount)> GetPagedTransactionsAsync(Guid? warehouseId, int pageNumber, int pageSize);
        Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<WarehouseTransfers>> GetAllTransfersAsync();
        Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId);
        Task CreateInTransactionAsync(CreateInventoryTransactionDto dto);
        Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto);
    }
}
