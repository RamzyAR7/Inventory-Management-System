using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(Guid warehouseId)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product
            };

            // Apply filter only if warehouseId is non-empty
            if (warehouseId != Guid.Empty)
            {
                return await _unitOfWork.InventoryTransactions.FindAsync(t => t.WarehouseID == warehouseId, includes);
            }

            // Fetch all transactions without filter
            return await _unitOfWork.InventoryTransactions.GetAllAsync(includes);
        }

        public async Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product
            };

            var transaction = await _unitOfWork.InventoryTransactions.GetByIdAsync(t => t.TransactionID == transactionId, includes);
            if (transaction == null)
                throw new Exception("Transaction not found.");
            return transaction;
        }

        public async Task<IEnumerable<WarehouseTransfers>> GetAllTransfersAsync()
        {
            var includes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.Product
            };

            return await _unitOfWork.WarehouseTransfers.GetAllAsync(includes);
        }

        public async Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId)
        {
            var includes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.Product,
                t => t.OutTransaction,
                t => t.InTransaction
            };

            var transfer = await _unitOfWork.WarehouseTransfers.GetByIdAsync(t => t.WarehouseTransferID == transferId, includes);
            if (transfer == null)
                throw new Exception("Transfer not found.");
            return transfer;
        }

        public async Task CreateInOrOutTransactionAsync(CreateInventoryTransactionDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new Exception("Warehouse not found.");

            var userRole = await GetCurrentUserRoleAsync();
            if (userRole == "Manager" && warehouse.ManagerID != Guid.Parse(GetCurrentUserId()))
                throw new UnauthorizedAccessException("You can only make transactions for your own warehouse.");

            var transaction = _mapper.Map<InventoryTransaction>(dto);
            transaction.TransactionID = Guid.NewGuid();
            transaction.TransactionDate = DateTime.UtcNow;

            await _unitOfWork.InventoryTransactions.AddAsync(transaction);

            int quantityChange = dto.Type == TransactionType.In ? dto.Quantity : -dto.Quantity;

            await UpdateWarehouseStockAsync(dto.WarehouseId, dto.ProductId, quantityChange);

            await _unitOfWork.SaveAsync(); // Use SaveAsync
        }

        public async Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto)
        {
            // Validate input
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Check warehouse existence
            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId);
            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId);
            if (fromWarehouse == null || toWarehouse == null)
                throw new KeyNotFoundException("Source or destination warehouse not found.");

            // Prevent transfer to the same warehouse
            if (fromWarehouse.WarehouseID == toWarehouse.WarehouseID)
                throw new InvalidOperationException("Cannot transfer to the same warehouse.");

            // Validate user permissions
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var userRole = await GetCurrentUserRoleAsync();
            if (userRole == "Manager")
            {
                if (fromWarehouse.ManagerID != Guid.Parse(userId))
                    throw new UnauthorizedAccessException("You can only transfer from your own warehouse.");
                if (toWarehouse.ManagerID != Guid.Parse(userId))
                    throw new UnauthorizedAccessException("You can only transfer to your own warehouse.");
            }

            // Validate product existence
            var productExists = await _unitOfWork.Products.ExistsAsync(dto.ProductId);
            if (!productExists)
                throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");

            // Check stock availability
            var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(dto.FromWarehouseId, dto.ProductId);
            int availableStock = stock?.StockQuantity ?? 0; // Assume 0 if no stock record exists
            if (availableStock < dto.Quantity)
                throw new InvalidOperationException($"Insufficient stock in the source warehouse '{fromWarehouse.WarehouseName}' for product with ID {dto.ProductId}. Available: {availableStock}, Requested: {dto.Quantity}");

            // Begin transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create out transaction
                var outTransaction = new InventoryTransaction
                {
                    TransactionID = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.Out,
                    WarehouseID = fromWarehouse.WarehouseID,
                    ProductID = dto.ProductId,
                    Quantity = dto.Quantity
                };

                // Create in transaction
                var inTransaction = new InventoryTransaction
                {
                    TransactionID = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.In,
                    WarehouseID = toWarehouse.WarehouseID,
                    ProductID = dto.ProductId,
                    Quantity = dto.Quantity
                };

                // Create warehouse transfer
                var transfer = new WarehouseTransfers
                {
                    WarehouseTransferID = Guid.NewGuid(),
                    TransferDate = DateTime.UtcNow,
                    FromWarehouseID = fromWarehouse.WarehouseID,
                    ToWarehouseID = toWarehouse.WarehouseID,
                    ProductID = dto.ProductId,
                    Quantity = dto.Quantity,
                    OutTransactionID = outTransaction.TransactionID,
                    InTransactionID = inTransaction.TransactionID
                };

                // Add entities
                await _unitOfWork.InventoryTransactions.AddAsync(outTransaction);
                await _unitOfWork.InventoryTransactions.AddAsync(inTransaction);
                await _unitOfWork.WarehouseTransfers.AddAsync(transfer);

                // Update stock
                await UpdateWarehouseStockAsync(fromWarehouse.WarehouseID, dto.ProductId, -dto.Quantity);
                await UpdateWarehouseStockAsync(toWarehouse.WarehouseID, dto.ProductId, dto.Quantity);

                // Save changes
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Concurrency error occurred during transfer: {Message}", ex.Message);
                throw new Exception("Concurrency error occurred during transfer. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Database error occurred during transfer: {Message}", ex.InnerException?.Message ?? ex.Message);
                throw new Exception($"Database error occurred during transfer: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "General error during transfer: {Message}", ex.Message);
                throw;
            }
        }

        private async Task UpdateWarehouseStockAsync(Guid warehouseId, Guid productId, int quantityChange)
        {
            var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(warehouseId, productId);
            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    WarehouseID = warehouseId,
                    ProductID = productId,
                    StockQuantity = 0
                };
                await _unitOfWork.WarehouseStocks.AddAsync(stock);
            }

            stock.StockQuantity += quantityChange;
            if (stock.StockQuantity < 0)
                throw new InvalidOperationException("Insufficient stock.");

            // Use the new UpdateAsync method for composite key entities
            await _unitOfWork.WarehouseStocks.UpdateAsync(stock);

            await _unitOfWork.Save();
        }

        private async Task<string> GetCurrentUserRoleAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == Guid.Parse(userId));
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user.Role;
        }

        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");
            return userId;
        }
    }
}
