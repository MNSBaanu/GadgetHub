using AutoMapper;
using GadgetHubAPI.Model;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductReadDTO>();
        }
    }
}
