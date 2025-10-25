using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Services;
using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributorController : ControllerBase
    {
        private readonly DistributorService _distributorService;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(
            DistributorService distributorService,
            QuotationComparisonRepo quotationComparisonRepo,
            ILogger<DistributorController> logger)
        {
            _distributorService = distributorService;
            _quotationComparisonRepo = quotationComparisonRepo;
            _logger = logger;
        }

        [HttpGet("list")]
        public ActionResult<List<DistributorInfoDTO>> GetDistributors()
        {
            try
            {
                var distributors = new List<DistributorInfoDTO>
                {
                    new DistributorInfoDTO
                    {
                        Name = "ElectroCom",
                        Url = "https://localhost:7077",
                        Status = "Active",
                        Description = "Electronics and gadgets distributor",
                        ContactInfo = "ElectroCom Support"
                    },
                    new DistributorInfoDTO
                    {
                        Name = "TechWorld",
                        Url = "https://localhost:7102",
                        Status = "Active",
                        Description = "Technology products distributor",
                        ContactInfo = "TechWorld Support"
                    },
                    new DistributorInfoDTO
                    {
                        Name = "GadgetCentral",
                        Url = "https://localhost:7007",
                        Status = "Active",
                        Description = "Central gadget distribution hub",
                        ContactInfo = "GadgetCentral Support"
                    }
                };

                return Ok(distributors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributors");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("performance")]
        public ActionResult<List<DistributorPerformanceDTO>> GetDistributorPerformance()
        {
            try
            {
                var quotations = _quotationComparisonRepo.GetQuotationComparisons();
                
                var performance = quotations
                    .GroupBy(q => q.DistributorName)
                    .Select(g => new DistributorPerformanceDTO
                    {
                        DistributorName = g.Key,
                        TotalQuotations = g.Count(),
                        SelectedQuotations = g.Count(q => q.Status == "Selected"),
                        AverageResponseTime = 2.5, // Mock data - would need actual tracking
                        AveragePrice = (decimal)g.Average(q => q.UnitPrice),
                        AverageDeliveryDays = (int)g.Average(q => q.EstimatedDeliveryDays),
                        SuccessRate = g.Count(q => q.Status == "Selected") * 100.0 / g.Count()
                    })
                    .ToList();

                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor performance");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("test-connection")]
        public async Task<ActionResult<DistributorTestResultDTO>> TestDistributorConnection([FromBody] DistributorTestRequestDTO request)
        {
            try
            {
                // Test connection to distributor
                var testResult = await _distributorService.TestConnection(request.DistributorUrl);
                
                var result = new DistributorTestResultDTO
                {
                    DistributorName = request.DistributorName,
                    Url = testResult.Url,
                    IsConnected = testResult.IsConnected,
                    TestTime = testResult.TestTime,
                    ResponseTime = testResult.ResponseTime
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing distributor connection");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class DistributorInfoDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }

    public class DistributorPerformanceDTO
    {
        public string DistributorName { get; set; } = string.Empty;
        public int TotalQuotations { get; set; }
        public int SelectedQuotations { get; set; }
        public double AverageResponseTime { get; set; }
        public decimal AveragePrice { get; set; }
        public int AverageDeliveryDays { get; set; }
        public double SuccessRate { get; set; }
    }


}

