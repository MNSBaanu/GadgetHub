namespace GadgetHubAPI.Configuration
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

        public ServiceUrlsOptions GetUrls()
        {
            if (IsProductionHost)
            {
                return new ServiceUrlsOptions
                {
                    ElectroCom = "http://gadgethub-electrocom.runasp.net",
                    TechWorld = "http://gadgethub-techworld.runasp.net",
                    GadgetCentral = "http://gadgethub-gadgetcentral.runasp.net"
                };
            }

            return new ServiceUrlsOptions
            {
                ElectroCom = _configuration["ServiceUrls:ElectroCom"] ?? "http://localhost:7077",
                TechWorld = _configuration["ServiceUrls:TechWorld"] ?? "http://localhost:7102",
                GadgetCentral = _configuration["ServiceUrls:GadgetCentral"] ?? "http://localhost:7007"
            };
        }
    }
}
