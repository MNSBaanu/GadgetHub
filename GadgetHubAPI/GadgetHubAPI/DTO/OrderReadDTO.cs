namespace GadgetHubAPI.DTO
{
    public class OrderReadDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemReadDTO> OrderItems { get; set; } = new List<OrderItemReadDTO>();
    }
    
}
