using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Extensions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests;

namespace Heureka_Metaheuristic_System_Backend_Api.MappingProfiles
{
    public class AlgorithmProfile : Profile
    {
        public AlgorithmProfile()
        {
            CreateMap<UpdateAlgorithmDto, Algorithm>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .IgnoreNullOrEmpty();
        }
    }
}
