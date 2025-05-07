using AutoMapper;
using IMS.BLL.DTOs.Transactions;
using IMS.BLL.Interfaces;
using IMS.BLL.SharedServices.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS.BLL.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IWhoIsUserLoginService _userLoginService;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IUnitOfWork unitOfWork, IProductHelperService productHelperService, IMapper mapper, IWhoIsUserLoginService userLoginService, ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _productHelperService = productHelperService;
            _mapper = mapper;
            _userLoginService = userLoginService;
            _logger = logger;
        }

        public async Task<(IEnumerable<InventoryTransaction> Items, int TotalCount)> GetPagedTransactionsAsync(
            Guid? warehouseId,
            int pageNumber,
            int pageSize,
            string searchSupplier = null,
            string searchCustomer = null)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product,
                t => t.Suppliers,
                t => t.Order,
                t => t.Order.Customer,
                t => t.InTransfers,
                t => t.OutTransfers
            };

            Expression<Func<InventoryTransaction, bool>> predicate = null;

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            _logger.LogInformation("GetPagedTransactionsAsync - User role: {UserRole}", userRole);

            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!managerWarehouseIds.Any())
                {
                    _logger.LogWarning("GetPagedTransactionsAsync - No warehouses assigned to Manager with UserID: {UserID}", userId);
                    return (Enumerable.Empty<InventoryTransaction>(), 0);
                }

                predicate = t => managerWarehouseIds.Contains(t.WarehouseID);

                if (warehouseId.HasValue)
                {
                    if (!managerWarehouseIds.Contains(warehouseId.Value))
                    {
                        _logger.LogWarning("GetPagedTransactionsAsync - Manager attempted to access transactions for unauthorized WarehouseID: {WarehouseID}", warehouseId.Value);
                        throw new UnauthorizedAccessException("You can only view transactions for your own warehouses.");
                    }
                    predicate = t => t.WarehouseID == warehouseId.Value;
                }
            }
            else if (warehouseId.HasValue)
            {
                predicate = t => t.WarehouseID == warehouseId.Value;
            }

            // If predicate is null, fetch all transactions (no filtering by warehouse)
            var transactionsQuery = predicate != null
                ? await _unitOfWork.InventoryTransactions.FindAsync(predicate, includes)
                : await _unitOfWork.InventoryTransactions.GetAllAsync(includes);

            // Apply search filters
            if (!string.IsNullOrEmpty(searchSupplier))
            {
                transactionsQuery = transactionsQuery.Where(t => t.Type == TransactionType.In && t.Suppliers != null && t.Suppliers.SupplierName.ToLower().Contains(searchSupplier.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchCustomer))
            {
                transactionsQuery = transactionsQuery.Where(t => t.Type == TransactionType.Out && t.Order != null && t.Order.Customer != null && t.Order.Customer.FullName.ToLower().Contains(searchCustomer.ToLower()));
            }

            // Apply pagination manually since we need to filter after includes
            var totalCount = transactionsQuery.Count();
            var paginatedTransactions = transactionsQuery
                .OrderByDescending(t => t.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("GetPagedTransactionsAsync - Retrieved {ItemCount} transactions, TotalCount: {TotalCount}", paginatedTransactions.Count, totalCount);
            return (paginatedTransactions, totalCount);
        }

        public async Task<(IEnumerable<InventoryTransaction> InTransactions, IEnumerable<InventoryTransaction> OutTransactions, IEnumerable<WarehouseTransfers> Transfers)> GetLimitedTransactionsAndTransfersAsync(Guid? warehouseId)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product,
                t => t.Suppliers,
                t => t.Order,
                t => t.Order.Customer,
                t => t.InTransfers,
                t => t.OutTransfers
            };

            Expression<Func<InventoryTransaction, bool>> predicate = null;

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            _logger.LogInformation("GetLimitedTransactionsAndTransfersAsync - User role: {UserRole}", userRole);

            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!managerWarehouseIds.Any())
                {
                    _logger.LogWarning("GetLimitedTransactionsAndTransfersAsync - No warehouses assigned to Manager with UserID: {UserID}", userId);
                    return (Enumerable.Empty<InventoryTransaction>(), Enumerable.Empty<InventoryTransaction>(), Enumerable.Empty<WarehouseTransfers>());
                }

                predicate = t => managerWarehouseIds.Contains(t.WarehouseID);

                if (warehouseId.HasValue)
                {
                    if (!managerWarehouseIds.Contains(warehouseId.Value))
                    {
                        _logger.LogWarning("GetLimitedTransactionsAndTransfersAsync - Manager attempted to access transactions for unauthorized WarehouseID: {WarehouseID}", warehouseId.Value);
                        throw new UnauthorizedAccessException("You can only view transactions for your own warehouses.");
                    }
                    predicate = t => t.WarehouseID == warehouseId.Value;
                }
            }
            else if (warehouseId.HasValue)
            {
                predicate = t => t.WarehouseID == warehouseId.Value;
            }

            // If predicate is null, fetch all transactions (no filtering by warehouse)
            var transactionsQuery = predicate != null
                ? await _unitOfWork.InventoryTransactions.FindAsync(predicate, includes)
                : await _unitOfWork.InventoryTransactions.GetAllAsync(includes);

            var allTransactions = transactionsQuery.OrderByDescending(t => t.TransactionDate).ToList();

            // Split into In and Out transactions
            var supplierInTransactions = allTransactions
                .Where(t => t.Type == TransactionType.In && t.SuppliersID != null)
                .Take(4)
                .ToList();
            var customerOutTransactions = allTransactions
                .Where(t => t.Type == TransactionType.Out && t.OrderID != null)
                .Take(3)
                .ToList();

            // Fetch transfers
            var transfersQuery = await GetAllTransfersAsync();
            if (warehouseId.HasValue)
            {
                transfersQuery = transfersQuery.Where(t => t.FromWarehouseID == warehouseId.Value || t.ToWarehouseID == warehouseId.Value);
            }
            var limitedTransfers = transfersQuery.OrderByDescending(t => t.TransferDate).Take(3).ToList();

            return (supplierInTransactions, customerOutTransactions, limitedTransfers);
        }

        public async Task<(IEnumerable<WarehouseTransfers> Items, int TotalCount)> GetPagedTransfersAsync(Guid? warehouseId, int pageNumber, int pageSize)
        {
            var includes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.FromProduct,
                t => t.ToProduct,
            };

            Expression<Func<WarehouseTransfers, bool>> predicate = null;
            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            _logger.LogInformation("GetPagedTransfersAsync - User role: {UserRole}", userRole);

            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));
                if (!managerWarehouseIds.Any())
                {
                    _logger.LogWarning("GetPagedTransfersAsync - No warehouses assigned to Manager with UserID: {UserID}", userId);
                    return (Enumerable.Empty<WarehouseTransfers>(), 0);
                }

                predicate = t => managerWarehouseIds.Contains(t.FromWarehouseID) || managerWarehouseIds.Contains(t.ToWarehouseID);

                if (warehouseId.HasValue)
                {
                    if (!managerWarehouseIds.Contains(warehouseId.Value))
                    {
                        _logger.LogWarning("GetPagedTransfersAsync - Manager attempted to access transfers for unauthorized WarehouseID: {WarehouseID}", warehouseId.Value);
                        throw new UnauthorizedAccessException("You can only view transfers involving your own warehouses.");
                    }
                    predicate = t => t.FromWarehouseID == warehouseId.Value || t.ToWarehouseID == warehouseId.Value;
                }
            }
            else if (warehouseId.HasValue)
            {
                predicate = t => t.FromWarehouseID == warehouseId.Value || t.ToWarehouseID == warehouseId.Value;
            }

            // If predicate is null, fetch all transfers (no filtering by warehouse)
            var transfersQuery = predicate != null
                ? await _unitOfWork.WarehouseTransfers.FindAsync(predicate, includes)
                : await _unitOfWork.WarehouseTransfers.GetAllAsync(includes);

            var totalCount = transfersQuery.Count();
            var paginatedTransfers = transfersQuery
                .OrderByDescending(t => t.TransferDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("GetPagedTransfersAsync - Retrieved {ItemCount} transfers, TotalCount: {TotalCount}", paginatedTransfers.Count, totalCount);
            return (paginatedTransfers, totalCount);
        }

        public async Task<InventoryTransaction> GetTransactionByIdAsync(Guid transactionId)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product
            };

            var transaction = await _unitOfWork.InventoryTransactions.GetByExpressionAsync(t => t.TransactionID == transactionId, includes);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for TransactionID: {TransactionID}", transactionId);
                throw new Exception("Transaction not found.");
            }

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!managerWarehouseIds.Contains(transaction.WarehouseID))
                {
                    _logger.LogWarning("Manager with UserID {UserID} attempted to access transaction {TransactionID} in unauthorized warehouse {WarehouseID}", userId, transactionId, transaction.WarehouseID);
                    throw new UnauthorizedAccessException("You can only view transactions for your own warehouses.");
                }
            }

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

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            _logger.LogInformation("GetAllTransfersAsync - User role: {UserRole}", userRole);
            _logger.LogInformation("GetAllTransfersAsync - Manager UserID: {UserID}", userId);

            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));
                _logger.LogInformation("GetAllTransfersAsync - Manager Warehouse IDs: {WarehouseIds}", string.Join(", ", managerWarehouseIds));

                if (!managerWarehouseIds.Any())
                {
                    _logger.LogWarning("GetAllTransfersAsync - No warehouses assigned to Manager with UserID: {UserID}", userId);
                    return Enumerable.Empty<WarehouseTransfers>();
                }

                var transfers = await _unitOfWork.WarehouseTransfers.FindAsync(
                    t => managerWarehouseIds.Contains(t.FromWarehouseID) || managerWarehouseIds.Contains(t.ToWarehouseID),
                    includes);
                _logger.LogInformation("GetAllTransfersAsync - Filtered transfers for Manager: {TransferCount}", transfers.Count());
                return transfers;
            }

            var allTransfers = await _unitOfWork.WarehouseTransfers.GetAllAsync(includes);
            _logger.LogInformation("GetAllTransfersAsync - All transfers retrieved: {TransferCount}", allTransfers.Count());
            return allTransfers;
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

            var transfer = await _unitOfWork.WarehouseTransfers.GetByExpressionAsync(t => t.WarehouseTransferID == transferId, includes);
            if (transfer == null)
            {
                _logger.LogWarning("Transfer not found for TransferID: {TransferID}", transferId);
                throw new Exception("Transfer not found.");
            }

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            if (userRole == "Manager")
            {
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!managerWarehouseIds.Contains(transfer.FromWarehouseID) && !managerWarehouseIds.Contains(transfer.ToWarehouseID))
                {
                    _logger.LogWarning("Manager with UserID {UserID} attempted to access transfer {TransferID} involving unauthorized warehouses From: {FromWarehouseID}, To: {ToWarehouseID}", userId, transferId, transfer.FromWarehouseID, transfer.ToWarehouseID);
                    throw new UnauthorizedAccessException("You can only view transfers involving your own warehouses.");
                }
            }

            return transfer;
        }

        public async Task CreateInTransactionAsync(CreateInventoryTransactionDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                _logger.LogWarning("Warehouse not found for WarehouseID: {WarehouseID}", dto.WarehouseId);
                throw new Exception("Warehouse not found.");
            }

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierID);
            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found for SupplierID: {SupplierID}", dto.SupplierID);
                throw new Exception("Supplier not found.");
            }

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            if (userRole == "Manager" && warehouse.ManagerID != Guid.Parse(userId))
            {
                _logger.LogWarning("Manager attempted to create transaction in unauthorized warehouse {WarehouseID}", dto.WarehouseId);
                throw new UnauthorizedAccessException("You can only make transactions for your own warehouse.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for ProductID: {ProductID}", dto.ProductId);
                throw new KeyNotFoundException($"Product with ID '{dto.ProductId}' not found.");
            }

            if (dto.ProductId != product.ProductID)
            {
                _logger.LogWarning("Product ID mismatch. Provided: {ProvidedID}, Retrieved: {RetrievedID}", dto.ProductId, product.ProductID);
                throw new InvalidOperationException("The provided ProductId does not match the retrieved ProductId.");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var inventoryTransaction = _mapper.Map<InventoryTransaction>(dto);
                inventoryTransaction.TransactionID = Guid.NewGuid();
                inventoryTransaction.TransactionDate = DateTime.UtcNow;
                inventoryTransaction.Type = TransactionType.In;

                await _unitOfWork.InventoryTransactions.AddAsync(inventoryTransaction);

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
                await _productHelperService.UpdateWarehouseStockAsync(dto.WarehouseId, dto.ProductId, quantityChange);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("In transaction completed successfully for TransactionID: {TransactionID}", inventoryTransaction.TransactionID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error during in transaction: {Message}", ex.Message);
                throw new Exception("Failed to complete the in transaction. Please try again.", ex);
            }
        }

        public async Task TransferBetweenWarehousesAsync(CreateWarehouseTransferDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Transfer DTO is null");
                throw new ArgumentNullException(nameof(dto));
            }
            if (dto.Quantity <= 0)
            {
                _logger.LogWarning("Invalid transfer quantity: {Quantity}", dto.Quantity);
                throw new ArgumentException("Transfer quantity must be positive.", nameof(dto.Quantity));
            }
            if (dto.ToProductId == Guid.Empty)
            {
                _logger.LogWarning("Destination ProductID is empty");
                throw new ArgumentException("Destination product ID is required.", nameof(dto.ToProductId));
            }

            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId);
            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId);
            if (fromWarehouse == null || toWarehouse == null)
            {
                _logger.LogWarning("Warehouse not found. FromWarehouseID: {FromWarehouseID}, ToWarehouseID: {ToWarehouseID}", dto.FromWarehouseId, dto.ToWarehouseId);
                throw new KeyNotFoundException("Source or destination warehouse not found.");
            }

            _logger.LogInformation("Transfer details: FromWarehouseID: {FromWarehouseID}, ToWarehouseID: {ToWarehouseID}, FromProductID: {FromProductID}, ToProductID: {ToProductID}, Quantity: {Quantity}",
                fromWarehouse.WarehouseID, toWarehouse.WarehouseID, dto.FromProductId, dto.ToProductId, dto.Quantity);

            if (fromWarehouse.WarehouseID == toWarehouse.WarehouseID)
            {
                _logger.LogWarning("Attempted transfer to the same warehouse: {WarehouseID}", fromWarehouse.WarehouseID);
                throw new InvalidOperationException("Cannot transfer to the same warehouse.");
            }

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated for transfer");
                throw new UnauthorizedAccessException("User not authenticated.");
            }
            if (userRole == "Manager")
            {
                if (fromWarehouse.ManagerID != Guid.Parse(userId))
                {
                    _logger.LogWarning("Manager attempted to transfer from unauthorized warehouse {WarehouseID}", fromWarehouse.WarehouseID);
                    throw new UnauthorizedAccessException("You can only transfer from your own warehouse.");
                }
                if (toWarehouse.ManagerID != Guid.Parse(userId))
                {
                    _logger.LogWarning("Manager attempted to transfer to unauthorized warehouse {WarehouseID}", toWarehouse.WarehouseID);
                    throw new UnauthorizedAccessException("You can only transfer to your own warehouse.");
                }
            }

            var fromProduct = await _unitOfWork.Products.GetByIdAsync(dto.FromProductId);
            if (fromProduct == null)
            {
                _logger.LogWarning("Source product not found for ProductID: {ProductID}", dto.FromProductId);
                throw new KeyNotFoundException($"Source product with ID {dto.FromProductId} not found.");
            }

            var toProduct = await _unitOfWork.Products.GetByIdAsync(dto.ToProductId);
            if (toProduct == null)
            {
                _logger.LogWarning("Destination product not found for ProductID: {ProductID}", dto.ToProductId);
                throw new KeyNotFoundException($"Destination product with ID {dto.ToProductId} not found.");
            }

            if (toProduct.ProductName.ToLower() != fromProduct.ProductName.ToLower())
            {
                _logger.LogWarning("Product name mismatch. Source: {SourceProductName}, Destination: {DestProductName}", fromProduct.ProductName, toProduct.ProductName);
                throw new InvalidOperationException($"Source and destination products must have the same name. Source: {fromProduct.ProductName}, Destination: {toProduct.ProductName}");
            }

            var fromStock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(dto.FromWarehouseId, dto.FromProductId);
            int availableStock = fromStock?.StockQuantity ?? 0;
            if (fromStock == null)
            {
                _logger.LogWarning("Source product {ProductName} not assigned to warehouse {WarehouseName}", fromProduct.ProductName, fromWarehouse.WarehouseName);
                throw new InvalidOperationException($"Source product '{fromProduct.ProductName}' is not assigned to warehouse '{fromWarehouse.WarehouseName}'.");
            }
            if (availableStock < dto.Quantity)
            {
                _logger.LogWarning("Insufficient stock in warehouse {WarehouseName} for product {ProductName}. Available: {Available}, Requested: {Requested}", fromWarehouse.WarehouseName, fromProduct.ProductName, availableStock, dto.Quantity);
                throw new InvalidOperationException($"Insufficient stock in the source warehouse '{fromWarehouse.WarehouseName}' for product '{fromProduct.ProductName}'. Available: {availableStock}, Requested: {dto.Quantity}");
            }

            var toWarehouseStock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(toWarehouse.WarehouseID, dto.ToProductId);
            if (toWarehouseStock == null)
            {
                _logger.LogWarning("Destination product {ProductName} not assigned to warehouse {WarehouseName}", toProduct.ProductName, toWarehouse.WarehouseName);
                throw new InvalidOperationException($"Destination product '{toProduct.ProductName}' is not assigned to warehouse '{toWarehouse.WarehouseName}'.");
            }

            _logger.LogInformation("toWarehouseStock exists: {Exists}, ProductID: {ProductID}, StockQuantity: {StockQuantity}",
                toWarehouseStock != null, dto.ToProductId, toWarehouseStock?.StockQuantity ?? 0);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var outTransaction = new InventoryTransaction
                {
                    TransactionID = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.Out,
                    WarehouseID = fromWarehouse.WarehouseID,
                    ProductID = fromProduct.ProductID,
                    Quantity = dto.Quantity
                };

                var inTransaction = new InventoryTransaction
                {
                    TransactionID = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.In,
                    WarehouseID = toWarehouse.WarehouseID,
                    ProductID = dto.ToProductId,
                    Quantity = dto.Quantity
                };

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

                await _unitOfWork.InventoryTransactions.AddAsync(outTransaction);
                await _unitOfWork.InventoryTransactions.AddAsync(inTransaction);
                await _unitOfWork.WarehouseTransfers.AddAsync(warehouseTransfer);

                await _productHelperService.UpdateWarehouseStockAsync(fromWarehouse.WarehouseID, dto.FromProductId, -dto.Quantity);
                await _productHelperService.UpdateWarehouseStockAsync(toWarehouse.WarehouseID, dto.ToProductId, dto.Quantity);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Transfer completed successfully for WarehouseTransferID: {WarehouseTransferID}", warehouseTransfer.WarehouseTransferID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error during transfer: {Message}", ex.Message);
                throw;
            }
        }
    }
}
