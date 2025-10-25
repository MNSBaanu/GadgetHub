using System;

namespace GadgetHubAPI.DTO
{
    public class DistributorTestResultDTO
    {
        public string DistributorName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DateTime TestTime { get; set; }
        public int ResponseTime { get; set; }
    }
}
