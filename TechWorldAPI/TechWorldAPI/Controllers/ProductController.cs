using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TechWorldAPI.Model;
using TechWorldAPI.DTO;
using TechWorldAPI.Data;

namespace TechWorldAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ProductRepo _productRepo;

        public ProductController(IMapper mapper, ProductRepo productRepo)
        {
            _mapper = mapper;
            _productRepo = productRepo;
        }

        [HttpGet]
        public ActionResult<List<ProductReadDTO>> GetProducts()
        {
            var products = _productRepo.GetProducts();
            return Ok(_mapper.Map<List<ProductReadDTO>>(products));
        }

        [HttpGet("{id}")]
        public ActionResult<ProductReadDTO> GetProduct(int id)
        {
            var product = _productRepo.GetProductById(id);
            if (product == null)
                return NotFound();

            return Ok(_mapper.Map<ProductReadDTO>(product));
        }

        [HttpPost]
        public ActionResult AddProduct(ProductWriteDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            if (_productRepo.AddProduct(product))
                return Ok();
            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateProduct(int id, ProductWriteDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            product.Id = id;
            if (_productRepo.UpdateProduct(product))
                return Ok();
            return NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var product = _productRepo.GetProductById(id);
            if (product == null)
                return NotFound();

            if (_productRepo.DeleteProduct(product))
                return Ok();
            return BadRequest();
        }

        [HttpGet("category/{category}")]
        public ActionResult<List<ProductReadDTO>> GetProductsByCategory(string category)
        {
            var products = _productRepo.GetProductsByCategory(category);
            return Ok(_mapper.Map<List<ProductReadDTO>>(products));
        }

        [HttpGet("search")]
        public ActionResult<List<ProductReadDTO>> SearchProducts([FromQuery] string term)
        {
            var products = _productRepo.SearchProducts(term);
            return Ok(_mapper.Map<List<ProductReadDTO>>(products));
        }
    }
}
