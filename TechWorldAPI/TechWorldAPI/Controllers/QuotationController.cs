using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TechWorldAPI.Model;
using TechWorldAPI.DTO;
using TechWorldAPI.Data;

namespace TechWorldAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly QuotationRepo _quotationRepo;

        public QuotationController(IMapper mapper, QuotationRepo quotationRepo)
        {
            _mapper = mapper;
            _quotationRepo = quotationRepo;
        }

        [HttpPost("request")]
        public ActionResult<QuotationResponseDTO> RequestQuotation(QuotationRequestDTO request)
        {
            var quotation = _quotationRepo.CreateQuotation(request);
            if (quotation == null)
                return BadRequest("Unable to create quotation. Product not found or insufficient stock.");

            return Ok(_mapper.Map<QuotationResponseDTO>(quotation));
        }

        [HttpGet]
        public ActionResult<List<QuotationResponseDTO>> GetQuotations()
        {
            var quotations = _quotationRepo.GetQuotations();
            return Ok(_mapper.Map<List<QuotationResponseDTO>>(quotations));
        }

        [HttpGet("{id}")]
        public ActionResult<QuotationResponseDTO> GetQuotation(int id)
        {
            var quotation = _quotationRepo.GetQuotationById(id);
            if (quotation == null)
                return NotFound();

            return Ok(_mapper.Map<QuotationResponseDTO>(quotation));
        }

        [HttpPut("{id}/status")]
        public ActionResult UpdateQuotationStatus(int id, [FromBody] string status)
        {
            if (_quotationRepo.UpdateQuotationStatus(id, status))
                return Ok();
            return NotFound();
        }

        [HttpPut("update-status")]
        public ActionResult UpdateQuotationStatusById([FromBody] QuotationStatusUpdateDTO updateRequest)
        {
            if (_quotationRepo.UpdateQuotationStatus(updateRequest.QuotationId, updateRequest.Status))
                return Ok(new { message = "Quotation status updated successfully", quotationId = updateRequest.QuotationId, status = updateRequest.Status });
            return NotFound(new { message = "Quotation not found", quotationId = updateRequest.QuotationId });
        }
    }
}
