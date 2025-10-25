namespace ElectroComAPI.DTO
{
    public class QuotationStatusUpdateDTO
    {
        public int QuotationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
