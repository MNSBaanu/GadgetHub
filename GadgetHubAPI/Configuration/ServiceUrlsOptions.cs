namespace GadgetHubAPI.Configuration
{
    public class ServiceUrlsOptions
    {
        public const string SectionName = "ServiceUrls";

        public string ElectroCom { get; set; } = string.Empty;
        public string TechWorld { get; set; } = string.Empty;
        public string GadgetCentral { get; set; } = string.Empty;

        public string GetDistributorUrl(string distributorName) => distributorName switch
        {
            "ElectroCom" => ElectroCom,
            "TechWorld" => TechWorld,
            "GadgetCentral" => GadgetCentral,
            _ => throw new ArgumentException($"Unknown distributor: {distributorName}")
        };
    }
}
