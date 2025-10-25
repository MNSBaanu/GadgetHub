using System.Text.Json;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductComparisonDTO>> GetBestProductsByCategory(string category = "all")
        {
            try
            {
                var allProducts = new List<ProductComparisonDTO>();

                // Fetch products from all distributors
                var electroComProducts = await GetProductsFromElectroCom(category);
                var techWorldProducts = await GetProductsFromTechWorld(category);
                var gadgetCentralProducts = await GetProductsFromGadgetCentral(category);

                // Combine all products
                allProducts.AddRange(electroComProducts);
                allProducts.AddRange(techWorldProducts);
                allProducts.AddRange(gadgetCentralProducts);

                // Group by ProductId and find the best product using comprehensive scoring
                // Note: Scoring uses original prices (before markup) for fair comparison
                var bestProducts = allProducts
                    .GroupBy(p => p.ProductId)
                    .Select(group => SelectBestProductFromGroup(group.ToList()))
                    .Where(p => p != null)
                    .OrderBy(p => p!.Score)
                    .ToList();

                // Ensure balanced distribution by mixing products from different distributors
                bestProducts = EnsureBalancedDistribution(bestProducts.Where(p => p != null).Cast<ProductComparisonDTO>().ToList());

                // Filter by category if not "all"
                if (category != "all")
                {
                    bestProducts = bestProducts.Where(p => p?.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                return bestProducts.Where(p => p != null).Cast<ProductComparisonDTO>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching best products");
                return new List<ProductComparisonDTO>();
            }
        }

        private async Task<List<ProductComparisonDTO>> GetProductsFromElectroCom(string category)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                var response = await _httpClient.GetAsync($"https://localhost:7077/api/product?category={category}", cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductReadDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return products?.Select(p => new ProductComparisonDTO
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        OriginalPrice = p.Price, // Store original price for scoring
                        Price = ApplyProfitMargin(p.Price), // Apply 20% profit margin for display
                        Stock = p.Stock,
                        Category = p.Category,
                        DistributorName = "ElectroCom",
                        DeliveryTime = 3, // 3 days delivery
                        ImageUrl = GetProductImageUrl(p.Category, p.Name)
                    }).ToList() ?? new List<ProductComparisonDTO>();
                }
                else
                {
                    _logger.LogWarning($"ElectroCom API returned {response.StatusCode} for category {category}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"ElectroCom API timeout for category {category}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from ElectroCom");
            }
            return new List<ProductComparisonDTO>();
        }

        private async Task<List<ProductComparisonDTO>> GetProductsFromTechWorld(string category)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                var response = await _httpClient.GetAsync($"https://localhost:7102/api/product?category={category}", cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductReadDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return products?.Select(p => new ProductComparisonDTO
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        OriginalPrice = p.Price, // Store original price for scoring
                        Price = ApplyProfitMargin(p.Price), // Apply 20% profit margin for display
                        Stock = p.Stock,
                        Category = p.Category,
                        DistributorName = "TechWorld",
                        DeliveryTime = 5, // 5 days delivery
                        ImageUrl = GetProductImageUrl(p.Category, p.Name)
                    }).ToList() ?? new List<ProductComparisonDTO>();
                }
                else
                {
                    _logger.LogWarning($"TechWorld API returned {response.StatusCode} for category {category}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"TechWorld API timeout for category {category}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from TechWorld");
            }
            return new List<ProductComparisonDTO>();
        }

        private async Task<List<ProductComparisonDTO>> GetProductsFromGadgetCentral(string category)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                var response = await _httpClient.GetAsync($"https://localhost:7007/api/product?category={category}", cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductReadDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return products?.Select(p => new ProductComparisonDTO
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        OriginalPrice = p.Price, // Store original price for scoring
                        Price = ApplyProfitMargin(p.Price), // Apply 20% profit margin for display
                        Stock = p.Stock,
                        Category = p.Category,
                        DistributorName = "GadgetCentral",
                        DeliveryTime = 4, // 4 days delivery
                        ImageUrl = GetProductImageUrl(p.Category, p.Name)
                    }).ToList() ?? new List<ProductComparisonDTO>();
                }
                else
                {
                    _logger.LogWarning($"GadgetCentral API returned {response.StatusCode} for category {category}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"GadgetCentral API timeout for category {category}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from GadgetCentral");
            }
            return new List<ProductComparisonDTO>();
        }

        private ProductComparisonDTO? SelectBestProductFromGroup(List<ProductComparisonDTO> products)
        {
            if (products == null || !products.Any())
                return null;

            // If only one product, return it
            if (products.Count == 1)
            {
                var product = products.First();
                product.Score = CalculateProductScore(product.OriginalPrice, product.Stock, product.DeliveryTime);
                return product;
            }

            // Calculate comprehensive score for each product using original prices
            var scoredProducts = products.Select(p => new
            {
                Product = p,
                Score = CalculateProductScore(p.OriginalPrice, p.Stock, p.DeliveryTime),
                PriceScore = CalculatePriceScore(p.OriginalPrice, products.Select(x => x.OriginalPrice).ToList()),
                StockScore = CalculateStockScore(p.Stock),
                DeliveryScore = CalculateDeliveryScore(p.DeliveryTime, products.Select(x => x.DeliveryTime).ToList())
            }).ToList();

            // Select the product with the highest overall score
            var bestProduct = scoredProducts.OrderByDescending(sp => sp.Score).First();
            
            // Add detailed scoring information to the product
            bestProduct.Product.Score = bestProduct.Score;
            bestProduct.Product.PriceScore = bestProduct.PriceScore;
            bestProduct.Product.StockScore = bestProduct.StockScore;
            bestProduct.Product.DeliveryScore = bestProduct.DeliveryScore;
            bestProduct.Product.IsBestChoice = true;
            
            // Generate comparison details and explanation
            bestProduct.Product.ComparisonDetails = $"Best choice from {bestProduct.Product.DistributorName}";
            bestProduct.Product.WhyBest = "Best Overall Value";

            return bestProduct.Product;
        }

        private decimal CalculateProductScore(decimal price, int stock, int deliveryTime)
        {
            // Weighted scoring system (total possible score: 100)
            var priceWeight = 0.4m;    // 40% weight for price
            var stockWeight = 0.3m;    // 30% weight for stock
            var deliveryWeight = 0.3m;  // 30% weight for delivery

            // Normalize scores to 0-100 scale
            var priceScore = CalculatePriceScore(price, new List<decimal> { price });
            var stockScore = CalculateStockScore(stock);
            var deliveryScore = CalculateDeliveryScore(deliveryTime, new List<int> { deliveryTime });

            // Calculate weighted total score
            var totalScore = (priceScore * priceWeight) + (stockScore * stockWeight) + (deliveryScore * deliveryWeight);
            
            return Math.Round(totalScore, 2);
        }

        private decimal CalculatePriceScore(decimal price, List<decimal> allPrices)
        {
            if (!allPrices.Any()) return 0;

            var minPrice = allPrices.Min();
            var maxPrice = allPrices.Max();
            
            if (maxPrice == minPrice) return 100; // All prices are the same

            // Lower price gets higher score (inverted scale)
            var normalizedScore = ((maxPrice - price) / (maxPrice - minPrice)) * 100;
            return Math.Max(0, Math.Min(100, normalizedScore));
        }

        private decimal CalculateStockScore(int stock)
        {
            // Stock scoring: more stock = higher score
            // Scale: 0-50 stock = 0-100 score
            if (stock <= 0) return 0;
            if (stock >= 50) return 100;
            
            return (stock / 50m) * 100;
        }

        private decimal CalculateDeliveryScore(int deliveryTime, List<int> allDeliveryTimes)
        {
            if (!allDeliveryTimes.Any()) return 0;

            var minDelivery = allDeliveryTimes.Min();
            var maxDelivery = allDeliveryTimes.Max();
            
            if (maxDelivery == minDelivery) return 100; // All delivery times are the same

            // Faster delivery gets higher score (inverted scale)
            var normalizedScore = ((maxDelivery - deliveryTime) / (maxDelivery - minDelivery)) * 100;
            return Math.Max(0, Math.Min(100, normalizedScore));
        }


        private List<ProductComparisonDTO> EnsureBalancedDistribution(List<ProductComparisonDTO> products)
        {
            if (products.Count <= 3) return products;

            // Group products by distributor
            var distributorGroups = products.GroupBy(p => p.DistributorName).ToList();
            
            // If we have products from all distributors, ensure balanced representation
            if (distributorGroups.Count >= 2)
            {
                var balancedProducts = new List<ProductComparisonDTO>();
                var maxPerDistributor = Math.Max(1, products.Count / distributorGroups.Count);
                
                foreach (var group in distributorGroups)
                {
                    var distributorProducts = group.OrderByDescending(p => p.Score).Take(maxPerDistributor).ToList();
                    balancedProducts.AddRange(distributorProducts);
                }
                
                // Fill remaining slots with highest scoring products from any distributor
                var remainingSlots = products.Count - balancedProducts.Count;
                if (remainingSlots > 0)
                {
                    var remainingProducts = products
                        .Where(p => !balancedProducts.Contains(p))
                        .OrderByDescending(p => p.Score)
                        .Take(remainingSlots)
                        .ToList();
                    
                    balancedProducts.AddRange(remainingProducts);
                }
                
                return balancedProducts.OrderByDescending(p => p.Score).ToList();
            }
            
            return products;
        }

        private decimal ApplyProfitMargin(decimal originalPrice)
        {
            // Apply 20% profit margin to the original price
            return Math.Round(originalPrice * 1.20m, 2);
        }

        private string GetProductImageUrl(string category, string productName)
        {
            // Map product names to image files
            var imageMap = new Dictionary<string, string>
            {
                // iPhone Products
                {"iPhone 15 Pro Max", "iphone-15-pro-max.jpg"},
                {"iPhone 15 Pro", "iphone-15-pro.jpg"},
                {"iPhone 15", "iphone-15.jpg"},
                
                // Mac Products
                {"MacBook Pro 16-inch M3 Max", "macbook-pro-16.jpg"},
                {"MacBook Pro 14-inch M3", "macbook-pro-14.jpg"},
                {"MacBook Air 15-inch M2", "macbook-air-15.jpg"},
                
                // iPad Products
                {"iPad Pro 12.9-inch M2", "ipad-pro-12-9.jpg"},
                {"iPad Pro 11-inch M2", "ipad-pro-11.jpg"},
                {"iPad Air 10.9-inch M1", "ipad-air.jpg"},
                
                // Apple Watch Products
                {"Apple Watch Ultra 2", "apple-watch-ultra.jpg"},
                {"Apple Watch Series 9", "apple-watch-series-9.jpg"},
                {"Apple Watch SE 2nd Gen", "apple-watch-se.jpg"},
                
                // AirPods Products
                {"AirPods Max", "airpods-max.jpg"},
                {"AirPods Pro 2nd Gen", "airpods-pro.jpg"},
                {"AirPods 3rd Gen", "airpods-3.jpg"},
                
                // Apple TV Products
                {"Apple TV 4K 3rd Gen", "apple-tv-4k-3rd.jpg"},
                {"Apple TV 4K 2nd Gen", "apple-tv-4k-2nd.jpg"},
                {"Apple TV HD", "apple-tv-hd.jpg"},
                
                // Accessories
                {"Magic Keyboard for iPad Pro", "magic-keyboard.jpg"},
                {"Apple Pencil 2nd Gen", "apple-pencil.jpg"},
                {"MagSafe Charger", "magsafe-charger.jpg"}
            };

            if (imageMap.TryGetValue(productName, out var imageName))
            {
                return $"/assets/products/{imageName}?v={DateTime.Now.Ticks}";
            }

            // Fallback to category-based images
            return category.ToLower() switch
            {
                "iphone" => $"/assets/products/iphone-default.jpg?v={DateTime.Now.Ticks}",
                "mac" => $"/assets/products/mac-default.jpg?v={DateTime.Now.Ticks}",
                "ipad" => $"/assets/products/ipad-default.jpg?v={DateTime.Now.Ticks}",
                "apple watch" => $"/assets/products/apple-watch-default.jpg?v={DateTime.Now.Ticks}",
                "airpods" => $"/assets/products/airpods-default.jpg?v={DateTime.Now.Ticks}",
                "apple tv" => $"/assets/products/apple-tv-default.jpg?v={DateTime.Now.Ticks}",
                "accessories" => $"/assets/products/accessories-default.jpg?v={DateTime.Now.Ticks}",
                _ => $"/assets/products/default.jpg?v={DateTime.Now.Ticks}"
            };
        }
    }
}
