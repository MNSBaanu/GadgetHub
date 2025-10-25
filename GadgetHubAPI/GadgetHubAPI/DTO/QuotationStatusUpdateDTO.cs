namespace GadgetHubAPI.DTO
{
    public class QuotationStatusUpdateDTO
    {
        public int ProductId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
