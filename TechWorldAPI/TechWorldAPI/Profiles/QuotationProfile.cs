using AutoMapper;
using TechWorldAPI.Model;
using TechWorldAPI.DTO;

namespace TechWorldAPI.Profiles
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
