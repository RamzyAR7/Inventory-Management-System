using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
        {
            return await _unitOfWork.InventoryTransactions.GetAllAsync();
        }

        public async Task<InventoryTransaction?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.InventoryTransactions.GetByIdAsync(e => e.TransactionID == id);
        }

        public async Task CreateAsync(InventoryTransaction transaction)
        {
            await _unitOfWork.InventoryTransactions.AddAsync(transaction);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(InventoryTransaction transaction)
        {
            _unitOfWork.InventoryTransactions.UpdateAsync(transaction);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.InventoryTransactions.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
