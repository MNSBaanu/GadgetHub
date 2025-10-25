using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Models;

namespace GadgetHubWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                // Fetch products from GadgetHubAPI which compares prices across all distributors
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://localhost:7000/api/product");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return Ok(products);
                }
                else
                {
                    // Fallback to sample data if API is not available
                    return Ok(GetSampleProducts());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from GadgetHubAPI");
                // Fallback to sample data
                return Ok(GetSampleProducts());
            }
        }

        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "iPhone 15 Pro",
                    Description = "Latest iPhone with A17 Pro chip and titanium design",
                    Price = 999.99m,
                    Stock = 50,
                    Category = "iPhone"
                },
                new Product
                {
                    Id = 2,
                    Name = "iPhone 15",
                    Description = "New iPhone with A16 Bionic chip and Dynamic Island",
                    Price = 799.99m,
                    Stock = 45,
                    Category = "iPhone"
                },
                new Product
                {
                    Id = 3,
                    Name = "MacBook Air M3",
                    Description = "Ultra-thin laptop with M3 chip for maximum performance",
                    Price = 1299.99m,
                    Stock = 30,
                    Category = "Mac"
                },
                new Product
                {
                    Id = 4,
                    Name = "MacBook Pro 16-inch",
                    Description = "Professional laptop with M3 Pro chip and Liquid Retina XDR display",
                    Price = 2499.99m,
                    Stock = 20,
                    Category = "Mac"
                },
                new Product
                {
                    Id = 5,
                    Name = "iPad Pro 12.9-inch",
                    Description = "Professional tablet with M2 chip for creative professionals",
                    Price = 1099.99m,
                    Stock = 35,
                    Category = "iPad"
                },
                new Product
                {
                    Id = 6,
                    Name = "iPad Air",
                    Description = "Powerful tablet with M1 chip and all-day battery life",
                    Price = 599.99m,
                    Stock = 40,
                    Category = "iPad"
                },
                new Product
                {
                    Id = 7,
                    Name = "Apple Watch Series 9",
                    Description = "Advanced smartwatch with health features and S9 chip",
                    Price = 399.99m,
                    Stock = 60,
                    Category = "Apple Watch"
                },
                new Product
                {
                    Id = 8,
                    Name = "Apple Watch SE",
                    Description = "Essential smartwatch features at an incredible value",
                    Price = 249.99m,
                    Stock = 55,
                    Category = "Apple Watch"
                },
                new Product
                {
                    Id = 9,
                    Name = "AirPods Pro (2nd generation)",
                    Description = "Active noise cancellation with Adaptive Transparency",
                    Price = 249.99m,
                    Stock = 70,
                    Category = "AirPods"
                },
                new Product
                {
                    Id = 10,
                    Name = "AirPods (3rd generation)",
                    Description = "Spatial audio with dynamic head tracking",
                    Price = 179.99m,
                    Stock = 65,
                    Category = "AirPods"
                },
                new Product
                {
                    Id = 11,
                    Name = "Apple TV 4K (3rd generation)",
                    Description = "Streaming device with A15 Bionic chip and Siri Remote",
                    Price = 129.99m,
                    Stock = 45,
                    Category = "Apple TV"
                },
                new Product
                {
                    Id = 12,
                    Name = "Magic Keyboard",
                    Description = "Wireless keyboard with Touch ID and backlit keys",
                    Price = 149.99m,
                    Stock = 80,
                    Category = "Accessories"
                },
                new Product
                {
                    Id = 13,
                    Name = "Magic Mouse",
                    Description = "Wireless mouse with multi-touch surface and rechargeable battery",
                    Price = 99.99m,
                    Stock = 75,
                    Category = "Accessories"
                },
                new Product
                {
                    Id = 14,
                    Name = "Studio Display",
                    Description = "27-inch 5K Retina display with 12MP Ultra Wide camera",
                    Price = 1599.99m,
                    Stock = 25,
                    Category = "Accessories"
                }
            };
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            // Sample product data - in a real application, this would come from a database
            var product = new Product
            {
                Id = id,
                Name = "Sample Product",
                Description = "This is a sample product description",
                Price = 299.99m,
                Stock = 10,
                Category = "Electronics"
            };

            return Ok(product);
        }
    }
}
