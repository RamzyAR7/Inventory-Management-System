namespace Inventory_Management_System.Models.DTOs
{
    public class InventoryDto
    {
        public Guid Id { get; set; }                   
        public string Name { get; set; } = string.Empty;         
        public int Quantity { get; set; }                        
        public string Category { get; set; } = string.Empty;     
        public string WarehouseName { get; set; } = string.Empty; 
    }
}
