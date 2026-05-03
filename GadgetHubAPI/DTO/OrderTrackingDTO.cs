namespace GadgetHubAPI.DTO
{
    public class OrderTrackingDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public List<TrackingDetailDTO> TrackingDetails { get; set; } = new List<TrackingDetailDTO>();
        public string? Notes { get; set; }
    }

    public class TrackingDetailDTO
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class DeliveryEstimateDTO
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Distributors { get; set; } = new List<string>();
        public string DeliveryNotes { get; set; } = string.Empty;
    }
}

