using AutoMapper;
using elasticsearchApi.Controllers;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;

namespace Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            /*CreateMap<UserForUpdateDto, user>()
                .ForMember(u => u.PhoneNumber, opt => opt.MapFrom(x => x.phone));*/
            /*CreateMap<personDTO, AddPersonModel>();
            CreateMap<SearchPersonModel, AddPersonModel>();*/
            CreateMap<addNewPersonDTO, outPersonDTO>();
        }
    }
}
