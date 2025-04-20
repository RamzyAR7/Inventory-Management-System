namespace Inventory_Management_System.Entities
{
    public enum UserRole
    {
        Admin,
        Manager,
        Employee
    }
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
    public enum TransactionType
    {
        In,
        Out
    }
    public enum ShipmentStatus
    {
        InTransit,
        Delivered,
        Cancelled,
        Pending
    }
}
