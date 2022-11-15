using AutoMapper;
using elasticsearchApi.Controllers;
using elasticsearchApi.Models;

namespace Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            /*CreateMap<UserForUpdateDto, user>()
                .ForMember(u => u.PhoneNumber, opt => opt.MapFrom(x => x.phone));*/
            CreateMap<_nrsz_person, Person>();
            CreateMap<Person, _nrsz_person>();
            CreateMap<SearchPersonModel, Person>();
        }
    }
}
