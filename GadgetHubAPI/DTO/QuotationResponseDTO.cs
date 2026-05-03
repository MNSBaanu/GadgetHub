namespace GadgetHubAPI.DTO
{
    public class QuotationResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public string DistributorName { get; set; } = string.Empty;
    }
}
