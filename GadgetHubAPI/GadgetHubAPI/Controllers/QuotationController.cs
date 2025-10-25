using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Services;
using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;
using AutoMapper;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationController : ControllerBase
    {
        private readonly DistributorService _distributorService;
        private readonly QuotationComparisonRepo _quotationComparisonRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(
            DistributorService distributorService,
            QuotationComparisonRepo quotationComparisonRepo,
            IMapper mapper,
            ILogger<QuotationController> logger)
        {
            _distributorService = distributorService;
            _quotationComparisonRepo = quotationComparisonRepo;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("request")]
        public async Task<ActionResult<List<QuotationResponseDTO>>> RequestQuotations([FromBody] QuotationRequestWithOrderId request)
        {
            try
            {
                _logger.LogInformation($"Requesting quotations for {request.QuotationRequests.Count} products");
                
                // Get quotations from all distributors
                var quotations = await _distributorService.GetQuotationsFromAllDistributors(request.QuotationRequests);
                
                // Save quotation comparisons to database
                await _quotationComparisonRepo.SaveQuotationComparisons(quotations, request.OrderId);
                
                _logger.LogInformation($"Received {quotations.Count} quotations from distributors");
                
                return Ok(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting quotations");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("comparisons/{orderId}")]
        public async Task<ActionResult<List<QuotationComparisonDTO>>> GetQuotationComparisons(int orderId)
        {
            try
            {
                var comparisons = await _quotationComparisonRepo.GetByOrderId(orderId);
                var comparisonDTOs = _mapper.Map<List<QuotationComparisonDTO>>(comparisons);
                
                return Ok(comparisonDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotation comparisons");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("select-best")]
        public async Task<ActionResult<QuotationResponseDTO>> SelectBestQuotation([FromBody] QuotationSelectionDTO selection)
        {
            try
            {
                // Update the selected quotation status
                await _quotationComparisonRepo.SelectBestQuotation(selection.QuotationComparisonId);
                
                // Get the selected quotation details
                var selectedQuotation = await _quotationComparisonRepo.GetSelectedQuotation(selection.QuotationComparisonId);
                
                return Ok(selectedQuotation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting best quotation");
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
