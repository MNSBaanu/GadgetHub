namespace GadgetHubAPI.DTO
{
    public class QuotationRequestWithOrderId
    {
        public int OrderId { get; set; }
        public List<QuotationRequestDTO> QuotationRequests { get; set; } = new List<QuotationRequestDTO>();
    }
}
