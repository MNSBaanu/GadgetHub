using AutoMapper;
using GadgetCentralAPI.Model;
using GadgetCentralAPI.DTO;

namespace GadgetCentralAPI.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductReadDTO>();
            CreateMap<ProductWriteDTO, Product>();
        }
    }
}
