using AutoMapper;
using TechWorldAPI.Model;
using TechWorldAPI.DTO;

namespace TechWorldAPI.Profiles
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
