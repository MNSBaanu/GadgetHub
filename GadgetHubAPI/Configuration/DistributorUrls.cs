namespace GadgetHubAPI.Configuration
{
    public class DistributorUrls
    {
        public const string SectionName = "DistributorUrls";

        public string ElectroCom { get; set; } = "https://localhost:7077";
        public string TechWorld { get; set; } = "https://localhost:7102";
        public string GadgetCentral { get; set; } = "https://localhost:7007";

        public string GetUrl(string distributorName) => distributorName switch
        {
            "ElectroCom" => ElectroCom,
            "TechWorld" => TechWorld,
            "GadgetCentral" => GadgetCentral,
            _ => throw new ArgumentException($"Unknown distributor: {distributorName}")
        };
    }
}
