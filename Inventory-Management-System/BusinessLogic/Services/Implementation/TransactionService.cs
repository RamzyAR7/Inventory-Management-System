using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

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

            if (warehouseId != Guid.Empty)
            {
                return await _unitOfWork.InventoryTransactions.FindAsync(t => t.WarehouseID == warehouseId, includes);
            }

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
                t => t.FromProduct,
                t => t.ToProduct,
            };

            return await _unitOfWork.WarehouseTransfers.GetAllAsync(includes);
        }

        public async Task<WarehouseTransfers> GetTransferByIdAsync(Guid transferId)
        {
            var includes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.ToProduct,
                t => t.FromProduct,
                t => t.OutTransaction,
                t => t.InTransaction
            };

            var transfer = await _unitOfWork.WarehouseTransfers.GetByIdAsync(t => t.WarehouseTransferID == transferId, includes);
            if (transfer == null)
                throw new Exception("Transfer not found.");
            return transfer;
        }

        public async Task CreateInTransactionAsync(CreateInventoryTransactionDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new Exception("Warehouse not found.");

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierID);
            if (supplier == null)
                throw new Exception("Supplier not found.");

            var userRole = await GetCurrentUserRoleAsync();
            if (userRole == "Manager" && warehouse.ManagerID != Guid.Parse(GetCurrentUserId()))
                throw new UnauthorizedAccessException("You can only make transactions for your own warehouse.");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{dto.ProductId}' not found.");

            if (dto.ProductId != product.ProductID)
                throw new InvalidOperationException("The provided ProductId does not match the retrieved ProductId.");

            // Begin transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var inventoryTransaction = _mapper.Map<InventoryTransaction>(dto);
                inventoryTransaction.TransactionID = Guid.NewGuid();
                inventoryTransaction.TransactionDate = DateTime.UtcNow;
                inventoryTransaction.Type = TransactionType.In;

                await _unitOfWork.InventoryTransactions.AddAsync(inventoryTransaction);

                // Check if the SupplierProduct entry already exists
                var existingSupplierProduct = await _unitOfWork.SupplierProducts
                    .FirstOrDefaultAsync(sp => sp.ProductID == dto.ProductId && sp.SupplierID == dto.SupplierID);

                if (existingSupplierProduct == null)
                {
                    await _unitOfWork.SupplierProducts.AddAsync(new SupplierProduct
                    {
                        SupplierID = dto.SupplierID,
                        ProductID = dto.ProductId
                    });
                    _logger.LogInformation("New SupplierProduct entry created for ProductID: {ProductID}, SupplierID: {SupplierID}", dto.ProductId, dto.SupplierID);
                }
                else
                {
                    _logger.LogInformation("SupplierProduct entry already exists for ProductID: {ProductID}, SupplierID: {SupplierID}", dto.ProductId, dto.SupplierID);
                }

                int quantityChange = dto.Quantity;
                await UpdateWarehouseStockAsync(dto.WarehouseId, dto.ProductId, quantityChange);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("In transaction completed successfully for TransactionID: {TransactionID}", inventoryTransaction.TransactionID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during in transaction: {Message}", ex.Message);
                throw new Exception("Failed to complete the in transaction. Please try again.", ex);
            }
        }

        public async Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto)
        {
            // Validate input
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Quantity <= 0)
                throw new ArgumentException("Transfer quantity must be positive.", nameof(dto.Quantity));
            if (dto.ToProductId == Guid.Empty)
                throw new ArgumentException("Destination product ID is required.", nameof(dto.ToProductId));

            // Check warehouse existence
            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId);
            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId);
            if (fromWarehouse == null || toWarehouse == null)
                throw new KeyNotFoundException("Source or destination warehouse not found.");

            _logger.LogInformation("Transfer details: FromWarehouseID: {FromWarehouseID}, ToWarehouseID: {ToWarehouseID}, FromProductID: {FromProductID}, ToProductID: {ToProductID}, Quantity: {Quantity}",
                fromWarehouse.WarehouseID, toWarehouse.WarehouseID, dto.FromProductId, dto.ToProductId, dto.Quantity);

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
            var fromProduct = await _unitOfWork.Products.GetByIdAsync(dto.FromProductId);
            if (fromProduct == null)
                throw new KeyNotFoundException($"Source product with ID {dto.FromProductId} not found.");

            var toProduct = await _unitOfWork.Products.GetByIdAsync(dto.ToProductId);
            if (toProduct == null)
                throw new KeyNotFoundException($"Destination product with ID {dto.ToProductId} not found.");

            // Ensure the product names match (as enforced by the UI)
            if (toProduct.ProductName.ToLower() != fromProduct.ProductName.ToLower())
            {
                throw new InvalidOperationException($"Source and destination products must have the same name. Source: {fromProduct.ProductName}, Destination: {toProduct.ProductName}");
            }

            // Check stock availability in fromWarehouse
            var fromStock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(dto.FromWarehouseId, dto.FromProductId);
            int availableStock = fromStock?.StockQuantity ?? 0;
            if (fromStock == null)
                throw new InvalidOperationException($"Source product '{fromProduct.ProductName}' is not assigned to warehouse '{fromWarehouse.WarehouseName}'.");
            if (availableStock < dto.Quantity)
                throw new InvalidOperationException($"Insufficient stock in the source warehouse '{fromWarehouse.WarehouseName}' for product '{fromProduct.ProductName}'. Available: {availableStock}, Requested: {dto.Quantity}");

            // Verify the destination product exists in the destination warehouse
            var toWarehouseStock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(toWarehouse.WarehouseID, dto.ToProductId);
            if (toWarehouseStock == null)
            {
                throw new InvalidOperationException($"Destination product '{toProduct.ProductName}' is not assigned to warehouse '{toWarehouse.WarehouseName}'.");
            }

            _logger.LogInformation("toWarehouseStock exists: {Exists}, ProductID: {ProductID}, StockQuantity: {StockQuantity}",
                toWarehouseStock != null, dto.ToProductId, toWarehouseStock?.StockQuantity ?? 0);

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
                    ProductID = fromProduct.ProductID,
                    Quantity = dto.Quantity
                };

                // Create in transaction
                var inTransaction = new InventoryTransaction
                {
                    TransactionID = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.In,
                    WarehouseID = toWarehouse.WarehouseID,
                    ProductID = dto.ToProductId,
                    Quantity = dto.Quantity
                };

                // Create warehouse transfer
                var warehouseTransfer = new WarehouseTransfers
                {
                    WarehouseTransferID = Guid.NewGuid(),
                    TransferDate = DateTime.UtcNow,
                    FromWarehouseID = fromWarehouse.WarehouseID,
                    ToWarehouseID = toWarehouse.WarehouseID,
                    FromProductID = dto.FromProductId,
                    ToProductID = dto.ToProductId,
                    Quantity = dto.Quantity,
                    OutTransactionID = outTransaction.TransactionID,
                    InTransactionID = inTransaction.TransactionID
                };

                // Add entities
                await _unitOfWork.InventoryTransactions.AddAsync(outTransaction);
                await _unitOfWork.InventoryTransactions.AddAsync(inTransaction);
                await _unitOfWork.WarehouseTransfers.AddAsync(warehouseTransfer);

                // Update stock
                await UpdateWarehouseStockAsync(fromWarehouse.WarehouseID, dto.FromProductId, -dto.Quantity);
                await UpdateWarehouseStockAsync(toWarehouse.WarehouseID, dto.ToProductId, dto.Quantity);

                // Save changes
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Transfer completed successfully for WarehouseTransferID: {WarehouseTransferID}", warehouseTransfer.WarehouseTransferID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during transfer: {Message}", ex.Message);
                throw;
            }
        }
        private async Task UpdateWarehouseStockAsync(Guid warehouseId, Guid productId, int quantityChange)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(warehouseId, productId);
            if (stock == null)
            {
                _logger.LogInformation("Creating new WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}", warehouseId, productId);
                stock = new WarehouseStock
                {
                    WarehouseID = warehouseId,
                    ProductID = productId,
                    StockQuantity = 0
                };
                try
                {
                    await _unitOfWork.WarehouseStocks.AddAsync(stock);
                    await _unitOfWork.SaveAsync();
                    _logger.LogInformation("New WarehouseStock created successfully for WarehouseID: {WarehouseID}, ProductID: {ProductID}", warehouseId, productId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Failed to create WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}. InnerException: {InnerException}",
                        warehouseId, productId, ex.InnerException?.Message);
                    throw new Exception($"Failed to create WarehouseStock for WarehouseID: {warehouseId}, ProductID: {productId}. Ensure the warehouse and product exist.", ex);
                }
            }

            stock.StockQuantity += quantityChange;
            if (stock.StockQuantity < 0)
                throw new InvalidOperationException($"Insufficient stock in warehouse {warehouseId} for product {productId}. Available: {stock.StockQuantity - quantityChange}, Requested: {-quantityChange}");

            try
            {
                await _unitOfWork.WarehouseStocks.UpdateAsync(stock);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("WarehouseStock updated successfully for WarehouseID: {WarehouseID}, ProductID: {ProductID}, New StockQuantity: {StockQuantity}",
                    warehouseId, productId, stock.StockQuantity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}. InnerException: {InnerException}",
                    warehouseId, productId, ex.InnerException?.Message);
                throw new Exception($"Failed to update WarehouseStock for WarehouseID: {warehouseId}, ProductID: {productId}.", ex);
            }
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
