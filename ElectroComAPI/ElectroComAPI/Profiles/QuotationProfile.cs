using AutoMapper;
using ElectroComAPI.Model;
using ElectroComAPI.DTO;

namespace ElectroComAPI.Profiles
{
    public class QuotationProfile : Profile
    {
        public QuotationProfile()
        {
            CreateMap<Quotation, QuotationResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : ""));
        }
    }
}
