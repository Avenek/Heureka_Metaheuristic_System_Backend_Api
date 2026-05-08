using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;

namespace Heureka_Metaheuristic_System_Backend_Api.MappingProfiles
{
    public class SessionMappingProfile : Profile
    {
        public SessionMappingProfile()
        {
            CreateMap<Session, SessionDto>()
            .ForMember(
                dest => dest.AlgorithmIds,
                opt => opt.MapFrom(src => src.Tests
                    .Select(t => t.AlgorithmId.ToString())
                    .Distinct()
                    .ToList())
            )
            .ForMember(
                dest => dest.FitnessFunctionIds,
                opt => opt.MapFrom(src => src.Tests
                    .Select(t => t.FitnessFunctionId)
                    .Distinct()
                    .ToList())
            );
        }
    }
}
