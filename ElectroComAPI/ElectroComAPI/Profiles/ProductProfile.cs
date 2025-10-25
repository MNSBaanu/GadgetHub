using AutoMapper;
using ElectroComAPI.Model;
using ElectroComAPI.DTO;

namespace ElectroComAPI.Profiles
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
