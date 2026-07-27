namespace GadgetHubWeb.Services
{
    public class ServiceUrlResolver
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceUrlResolver(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsProductionHost =>
            (_httpContextAccessor.HttpContext?.Request.Host.Host ?? "")
            .Contains("runasp.net", StringComparison.OrdinalIgnoreCase);

        public string GadgetHubApi => Resolve(
            "ServiceUrls:GadgetHubApi",
            "http://gadgethub-gadgethub.runasp.net",
            "http://localhost:7091");

        public string ElectroCom => Resolve(
            "ServiceUrls:ElectroCom",
            "http://gadgethub-electrocom.runasp.net",
            "http://localhost:7077");

        public string TechWorld => Resolve(
            "ServiceUrls:TechWorld",
            "http://gadgethub-techworld.runasp.net",
            "http://localhost:7102");

        public string GadgetCentral => Resolve(
            "ServiceUrls:GadgetCentral",
            "http://gadgethub-gadgetcentral.runasp.net",
            "http://localhost:7007");

        private string Resolve(string configKey, string productionUrl, string localUrl)
        {
            if (IsProductionHost)
                return productionUrl;

            return _configuration[configKey] ?? localUrl;
        }
    }
}
