using Microsoft.AspNetCore.Mvc;
using GadgetHubAPI.Services;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductComparisonDTO>>> GetBestProducts([FromQuery] string category = "all")
        {
            try
            {
                _logger.LogInformation($"Fetching best products for category: {category}");
                
                var products = await _productService.GetBestProductsByCategory(category);
                
                _logger.LogInformation($"Found {products.Count} best products");
                
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching best products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductComparisonDTO>> GetProductById(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching product with ID: {id}");
                
                var products = await _productService.GetBestProductsByCategory("all");
                var product = products.FirstOrDefault(p => p.ProductId == id);
                
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                
                _logger.LogInformation($"Found product: {product.Name}");
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product by ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("categories")]
        public ActionResult<List<string>> GetCategories()
        {
            try
            {
                var categories = new List<string>
                {
                    "iPhone",
                    "Mac", 
                    "iPad",
                    "Apple Watch",
                    "AirPods",
                    "Apple TV",
                    "Accessories"
                };

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}