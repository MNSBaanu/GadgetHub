using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using GadgetHubAPI.Model;
using GadgetHubAPI.DTO;
using GadgetHubAPI.Data;

namespace GadgetHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly CustomerRepo _customerRepo;

        public CustomerController(IMapper mapper, CustomerRepo customerRepo)
        {
            _mapper = mapper;
            _customerRepo = customerRepo;
        }

        [HttpGet]
        public ActionResult<List<CustomerReadDTO>> GetCustomers()
        {
            var customers = _customerRepo.GetCustomers();
            return Ok(_mapper.Map<List<CustomerReadDTO>>(customers));
        }

        [HttpGet("{id}")]
        public ActionResult<CustomerReadDTO> GetCustomer(int id)
        {
            var customer = _customerRepo.GetCustomerById(id);
            if (customer == null)
                return NotFound();

            return Ok(_mapper.Map<CustomerReadDTO>(customer));
        }

        [HttpPost]
        public ActionResult AddCustomer(CustomerWriteDTO customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            if (_customerRepo.AddCustomer(customer))
                return Ok();
            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCustomer(int id, CustomerWriteDTO customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            customer.Id = id;
            if (_customerRepo.UpdateCustomer(customer))
                return Ok();
            return NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCustomer(int id)
        {
            var customer = _customerRepo.GetCustomerById(id);
            if (customer == null)
                return NotFound();

            if (_customerRepo.DeleteCustomer(customer))
                return Ok();
            return BadRequest();
        }
    }
}
