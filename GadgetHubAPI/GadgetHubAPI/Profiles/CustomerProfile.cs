using AutoMapper;
using GadgetHubAPI.Model;
using GadgetHubAPI.DTO;

namespace GadgetHubAPI.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerReadDTO>();
            CreateMap<CustomerWriteDTO, Customer>();
        }
    }
}
