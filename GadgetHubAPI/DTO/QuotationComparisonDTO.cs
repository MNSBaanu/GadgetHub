namespace GadgetHubAPI.DTO
{
    public class QuotationComparisonDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string DistributorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? Notes { get; set; }
    }
    
}
