using GadgetHubAPI.DTO;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace GadgetHubAPI.Services
{
    public class DistributorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DistributorService> _logger;

        public DistributorService(HttpClient httpClient, ILogger<DistributorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DistributorTestResultDTO> TestConnection(string distributorUrl)
        {
            var startTime = DateTime.UtcNow;
            var result = new DistributorTestResultDTO
            {
                Url = distributorUrl,
                TestTime = startTime,
                IsConnected = false,
                ResponseTime = 0
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // 10 second timeout for connection test
                var response = await _httpClient.GetAsync(distributorUrl, cts.Token);
                
                var endTime = DateTime.UtcNow;
                result.ResponseTime = (int)(endTime - startTime).TotalMilliseconds;
                result.IsConnected = response.IsSuccessStatusCode;
                
                _logger.LogInformation($"Connection test to {distributorUrl}: {(result.IsConnected ? "Success" : "Failed")} - {result.ResponseTime}ms");
            }
            catch (OperationCanceledException)
            {
                var endTime = DateTime.UtcNow;
                result.ResponseTime = (int)(endTime - startTime).TotalMilliseconds;
                result.IsConnected = false;
                _logger.LogWarning($"Connection test to {distributorUrl} timed out after {result.ResponseTime}ms");
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                result.ResponseTime = (int)(endTime - startTime).TotalMilliseconds;
                result.IsConnected = false;
                _logger.LogError(ex, $"Connection test to {distributorUrl} failed after {result.ResponseTime}ms");
            }

            return result;
        }

        public async Task<List<QuotationResponseDTO>> GetQuotationsFromAllDistributors(List<QuotationRequestDTO> requests)
        {
            var allQuotations = new List<QuotationResponseDTO>();

            _logger.LogInformation($"Requesting quotations from all distributors for {requests.Count} products");

            // Get quotations from ElectroCom
            var electroComQuotations = await GetQuotationsFromElectroCom(requests);
            allQuotations.AddRange(electroComQuotations);
            _logger.LogInformation($"ElectroCom returned {electroComQuotations.Count} quotations");

            // Get quotations from TechWorld
            var techWorldQuotations = await GetQuotationsFromTechWorld(requests);
            allQuotations.AddRange(techWorldQuotations);
            _logger.LogInformation($"TechWorld returned {techWorldQuotations.Count} quotations");

            // Get quotations from GadgetCentral
            var gadgetCentralQuotations = await GetQuotationsFromGadgetCentral(requests);
            allQuotations.AddRange(gadgetCentralQuotations);
            _logger.LogInformation($"GadgetCentral returned {gadgetCentralQuotations.Count} quotations");

            _logger.LogInformation($"Total quotations received: {allQuotations.Count}");
            return allQuotations;
        }

        private async Task<List<QuotationResponseDTO>> GetQuotationsFromElectroCom(List<QuotationRequestDTO> requests)
        {
            try
            {
                var quotations = new List<QuotationResponseDTO>();
                
                foreach (var request in requests)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                        var response = await _httpClient.PostAsJsonAsync("https://localhost:7077/api/Quotation/request", request, cts.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var quotation = await response.Content.ReadFromJsonAsync<QuotationResponseDTO>();
                            if (quotation != null)
                            {
                                quotation.DistributorName = "ElectroCom";
                                quotations.Add(quotation);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"ElectroCom API returned {response.StatusCode} for product {request.ProductId}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"ElectroCom API timeout for product {request.ProductId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting quotation from ElectroCom for product {request.ProductId}");
                    }
                }
                
                return quotations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations from ElectroCom");
                return new List<QuotationResponseDTO>();
            }
        }

        private async Task<List<QuotationResponseDTO>> GetQuotationsFromTechWorld(List<QuotationRequestDTO> requests)
        {
            try
            {
                var quotations = new List<QuotationResponseDTO>();
                
                foreach (var request in requests)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                        var response = await _httpClient.PostAsJsonAsync("https://localhost:7102/api/Quotation/request", request, cts.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var quotation = await response.Content.ReadFromJsonAsync<QuotationResponseDTO>();
                            if (quotation != null)
                            {
                                quotation.DistributorName = "TechWorld";
                                quotations.Add(quotation);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"TechWorld API returned {response.StatusCode} for product {request.ProductId}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"TechWorld API timeout for product {request.ProductId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting quotation from TechWorld for product {request.ProductId}");
                    }
                }
                
                return quotations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations from TechWorld");
                return new List<QuotationResponseDTO>();
            }
        }

        private async Task<List<QuotationResponseDTO>> GetQuotationsFromGadgetCentral(List<QuotationRequestDTO> requests)
        {
            try
            {
                var quotations = new List<QuotationResponseDTO>();
                
                foreach (var request in requests)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                        var response = await _httpClient.PostAsJsonAsync("https://localhost:7007/api/Quotation/request", request, cts.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var quotation = await response.Content.ReadFromJsonAsync<QuotationResponseDTO>();
                            if (quotation != null)
                            {
                                quotation.DistributorName = "GadgetCentral";
                                quotations.Add(quotation);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"GadgetCentral API returned {response.StatusCode} for product {request.ProductId}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"GadgetCentral API timeout for product {request.ProductId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting quotation from GadgetCentral for product {request.ProductId}");
                    }
                }
                
                return quotations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations from GadgetCentral");
                return new List<QuotationResponseDTO>();
            }
        }
    }
}
