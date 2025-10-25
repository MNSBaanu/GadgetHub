using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Model;

namespace GadgetHubAPI.Data
{
    public class QuotationComparisonRepo
    {
        private readonly GadgetHubDBContext _context;

        public QuotationComparisonRepo(GadgetHubDBContext context)
        {
            _context = context;
        }

        public List<QuotationComparison> GetQuotationComparisons()
        {
            return _context.QuotationComparisons
                .Include(qc => qc.Order)
                .ThenInclude(o => o!.Customer)
                .Include(qc => qc.Product)
                .OrderByDescending(qc => qc.CreatedDate) // Show most recent quotations first
                .ToList();
        }

        public List<QuotationComparison> GetQuotationComparisonsByOrderId(int orderId)
        {
            return _context.QuotationComparisons
                .Include(qc => qc.Order)
                .Include(qc => qc.Product)
                .Where(qc => qc.OrderId == orderId)
                .OrderByDescending(qc => qc.CreatedDate) // Show most recent quotations first
                .ToList();
        }

        public QuotationComparison? GetQuotationComparisonById(int id)
        {
            return _context.QuotationComparisons
                .Include(qc => qc.Order)
                .Include(qc => qc.Product)
                .FirstOrDefault(qc => qc.Id == id);
        }

        public bool AddQuotationComparison(QuotationComparison quotationComparison)
        {
            try
            {
                _context.QuotationComparisons.Add(quotationComparison);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateQuotationComparison(QuotationComparison quotationComparison)
        {
            try
            {
                _context.QuotationComparisons.Update(quotationComparison);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteQuotationComparison(QuotationComparison quotationComparison)
        {
            try
            {
                _context.QuotationComparisons.Remove(quotationComparison);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<QuotationComparison> GetBestQuotationsForOrder(int orderId)
        {
            return _context.QuotationComparisons
                .Include(qc => qc.Order)
                .Include(qc => qc.Product)
                .Where(qc => qc.OrderId == orderId && qc.Status == "Selected")
                .ToList();
        }

        public async Task SaveQuotationComparisons(List<DTO.QuotationResponseDTO> quotations, int orderId)
        {
            foreach (var quotation in quotations)
            {
                // Apply 20% markup to match frontend display prices
                var markedUpUnitPrice = Math.Round(quotation.UnitPrice * 1.20m, 2);
                var markedUpTotalPrice = Math.Round(quotation.TotalPrice * 1.20m, 2);
                
                var comparison = new QuotationComparison
                {
                    OrderId = orderId, // Add the missing OrderId
                    ProductId = quotation.ProductId,
                    DistributorName = quotation.DistributorName,
                    UnitPrice = markedUpUnitPrice, // Apply 20% markup to match frontend prices
                    AvailableStock = quotation.AvailableStock,
                    EstimatedDeliveryDays = quotation.EstimatedDeliveryDays,
                    TotalPrice = markedUpTotalPrice, // Apply 20% markup to match frontend prices
                    Status = "Received",
                    CreatedDate = DateTime.UtcNow,
                    Notes = quotation.Notes
                };
                
                _context.QuotationComparisons.Add(comparison);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task SelectBestQuotation(int quotationComparisonId)
        {
            var comparison = await _context.QuotationComparisons.FindAsync(quotationComparisonId);
            if (comparison != null)
            {
                comparison.Status = "Selected";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DTO.QuotationResponseDTO?> GetSelectedQuotation(int quotationComparisonId)
        {
            var comparison = await _context.QuotationComparisons
                .Include(qc => qc.Product)
                .FirstOrDefaultAsync(qc => qc.Id == quotationComparisonId);
                
            if (comparison == null) return null;
            
            return new DTO.QuotationResponseDTO
            {
                Id = comparison.Id,
                ProductId = comparison.ProductId,
                ProductName = comparison.Product?.Name ?? "",
                UnitPrice = comparison.UnitPrice,
                AvailableStock = comparison.AvailableStock,
                EstimatedDeliveryDays = comparison.EstimatedDeliveryDays,
                TotalPrice = comparison.TotalPrice,
                Status = comparison.Status,
                CreatedDate = comparison.CreatedDate,
                DistributorName = comparison.DistributorName,
                Notes = comparison.Notes
            };
        }

        public async Task<List<QuotationComparison>> GetByOrderId(int orderId)
        {
            return await _context.QuotationComparisons
                .Include(qc => qc.Order)
                .Include(qc => qc.Product)
                .Where(qc => qc.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<List<QuotationComparison>> GetByOrderIdAndProductId(int orderId, int productId)
        {
            return await _context.QuotationComparisons
                .Include(qc => qc.Order)
                .Include(qc => qc.Product)
                .Where(qc => qc.OrderId == orderId && qc.ProductId == productId)
                .ToListAsync();
        }

        public async Task UpdateSelectedDistributor(int quotationId, string distributorName)
        {
            // First, unselect all quotations for the same product
            var quotation = await _context.QuotationComparisons.FindAsync(quotationId);
            if (quotation != null)
            {
                var productQuotations = await _context.QuotationComparisons
                    .Where(qc => qc.ProductId == quotation.ProductId && qc.OrderId == quotation.OrderId)
                    .ToListAsync();

                foreach (var qc in productQuotations)
                {
                    qc.Status = "Received"; // Reset all to received
                }

                // Select the chosen distributor
                var selectedQuotation = productQuotations.FirstOrDefault(qc => qc.DistributorName == distributorName);
                if (selectedQuotation != null)
                {
                    selectedQuotation.Status = "Selected";
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
