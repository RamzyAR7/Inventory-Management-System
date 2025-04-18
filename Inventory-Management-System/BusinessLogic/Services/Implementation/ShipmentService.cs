using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class ShipmentService : IShipmentService
    {
        private readonly InventoryDbContext _context;

        public ShipmentService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync()
        {
            return await _context.Shipments
                .Include(s => s.Warehouse)
                .Include(s => s.Order)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments
                .Include(s => s.Warehouse)
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.ShipmentID == id);
        }

        public async Task CreateAsync(Shipment shipment)
        {
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Shipment shipment)
        {
            _context.Shipments.Update(shipment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment != null)
            {
                _context.Shipments.Remove(shipment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
