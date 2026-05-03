namespace GadgetHubAPI.DTO
{
    public class ProductComparisonDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; } // Final price with 20% markup (standardized)
        public decimal OriginalPrice { get; set; } // Original price from distributor
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string DistributorName { get; set; } = string.Empty;
        public int DeliveryTime { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        
        // Scoring properties for comparison
        public decimal Score { get; set; }
        public decimal PriceScore { get; set; }
        public decimal StockScore { get; set; }
        public decimal DeliveryScore { get; set; }
        public bool IsBestChoice { get; set; }
        
        // Comparison details
        public string ComparisonDetails { get; set; } = string.Empty;
        public string WhyBest { get; set; } = string.Empty;
    }

}
