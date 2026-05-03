using Microsoft.EntityFrameworkCore;
using ElectroComAPI.Model;
using ElectroComAPI.DTO;

namespace ElectroComAPI.Data
{
    public class QuotationRepo
    {
        private readonly ElectroComDBContext _context;

        public QuotationRepo(ElectroComDBContext context)
        {
            _context = context;
        }

        public Quotation? CreateQuotation(QuotationRequestDTO request)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == request.ProductId);
                if (product == null || product.Stock < request.Quantity)
                {
                    return null;
                }

                // ElectroCom's pricing logic - competitive pricing with bulk discounts
                decimal unitPrice = product.Price;
                if (request.Quantity >= 10)
                {
                    unitPrice *= 0.95m; // 5% discount for bulk orders
                }
                else if (request.Quantity >= 5)
                {
                    unitPrice *= 0.98m; // 2% discount for medium orders
                }

                var quotation = new Quotation
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = unitPrice,
                    AvailableStock = product.Stock,
                    EstimatedDeliveryDays = CalculateDeliveryDays(request.Quantity),
                    TotalPrice = unitPrice * request.Quantity,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddHours(24), // 24-hour validity
                    Notes = "ElectroCom - Fast delivery guaranteed"
                };

                _context.Quotations.Add(quotation);
                _context.SaveChanges();
                return quotation;
            }
            catch
            {
                return null;
            }
        }

        public List<Quotation> GetQuotations()
        {
            return _context.Quotations.Include(q => q.Product).ToList();
        }

        public Quotation? GetQuotationById(int id)
        {
            return _context.Quotations.Include(q => q.Product).FirstOrDefault(q => q.Id == id);
        }

        public bool UpdateQuotationStatus(int id, string status)
        {
            try
            {
                var quotation = _context.Quotations.FirstOrDefault(q => q.Id == id);
                if (quotation != null)
                {
                    quotation.Status = status;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private int CalculateDeliveryDays(int quantity)
        {
            // ElectroCom's delivery logic - faster for smaller quantities
            if (quantity <= 5)
                return 2; // 2 days for small orders
            else if (quantity <= 20)
                return 3; // 3 days for medium orders
            else
                return 5; // 5 days for large orders
        }
    }
}
