using AutoMapper;
using GadgetHubAPI.Model;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Profiles
{
    public class QuotationProfile : Profile
    {
        public QuotationProfile()
        {
            CreateMap<QuotationRequestDTO, QuotationRequestDTO>();
            
            CreateMap<QuotationResponseDTO, QuotationComparison>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.DistributorName, opt => opt.MapFrom(src => src.DistributorName))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.AvailableStock))
                .ForMember(dest => dest.EstimatedDeliveryDays, opt => opt.MapFrom(src => src.EstimatedDeliveryDays))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<QuotationComparison, QuotationComparisonDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : ""));

            CreateMap<QuotationComparison, QuotationResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : ""))
                .ForMember(dest => dest.Quantity, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.AvailableStock))
                .ForMember(dest => dest.EstimatedDeliveryDays, opt => opt.MapFrom(src => src.EstimatedDeliveryDays))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.DistributorName, opt => opt.MapFrom(src => src.DistributorName));
        }
    }
}
