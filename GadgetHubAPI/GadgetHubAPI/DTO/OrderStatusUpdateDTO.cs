namespace GadgetHubAPI.DTO
{
    public class OrderStatusUpdateDTO
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }
}
