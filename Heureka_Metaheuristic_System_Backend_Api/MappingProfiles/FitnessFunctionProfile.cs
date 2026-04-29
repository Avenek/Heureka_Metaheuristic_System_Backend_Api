using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using System.Text.Json;

namespace Heureka_Metaheuristic_System_Backend_Api.MappingProfiles
{
    public class FitnessFunctionProfile : Profile
    {
        public FitnessFunctionProfile()
        {
            CreateMap<FitnessFunction, FitnessFunctionDto>()
                .ForMember(
                    dest => dest.DomainPerVariable,
                    opt => opt.MapFrom(src =>
                        JsonSerializer.Deserialize<List<FitnessFunctionDomainDto>>(src.DomainPerVariable)
                    )
                );

            CreateMap<FitnessFunctionDto, FitnessFunction>()
                .ForMember(
                    dest => dest.DomainPerVariable,
                    opt => opt.MapFrom(src =>
                        JsonSerializer.Serialize(src.DomainPerVariable)
                    )
                );
        }
    }
}
