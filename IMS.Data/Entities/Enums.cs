namespace IMS.Data.Entities
{ 
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
    public enum DeliveryManStatus
    {
        Free,
        Busy
    }

    public enum DeliveryMethod
    {
        Delivering,
        Pickup
    }
}
