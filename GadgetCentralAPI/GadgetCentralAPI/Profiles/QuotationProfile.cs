using AutoMapper;
using GadgetCentralAPI.Model;
using GadgetCentralAPI.DTO;

namespace GadgetCentralAPI.Profiles
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
