using IMS.BLL.DTOs.Transactions;
using IMS.Domain.Entities;

namespace IMS.BLL.Interfaces
{
    public interface ITransactionService
    {
        Task<(IEnumerable<InventoryTransaction> Items, int TotalCount)> GetPagedTransactionsAsync(
             Guid? warehouseId,
             int pageNumber,
             int pageSize,
             string searchSupplier = null,
             string searchCustomer = null,
             string sortColumn = "TransactionDate",
             bool sortAscending = false);
        Task<(IEnumerable<InventoryTransaction> InTransactions, IEnumerable<InventoryTransaction> OutTransactions, IEnumerable<WarehouseTransfers> Transfers)> GetLimitedTransactionsAndTransfersAsync(Guid? warehouseId);
        Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<WarehouseTransfers>> GetAllTransfersAsync();
        Task<(IEnumerable<WarehouseTransfers> Items, int TotalCount)> GetPagedTransfersAsync(
                    Guid? warehouseId,
                    int pageNumber,
                    int pageSize,
                    string sortColumn = "TransferDate",
                    bool sortAscending = false);
        Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId);
        Task CreateInTransactionAsync(CreateInventoryTransactionDto dto);
        Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto);
    }
}
