using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.MappingProfiles
{
    internal sealed class DataDtoMappingService : IMappingService
    {
        public DataDtoMappingService(IMapper mapper, ILogger<DataDtoMappingService> logger)
        {
            Mapper = mapper;
            Logger = logger;
        }

        public ILogger Logger { get; }
        public IMapper Mapper { get; }
    }

}
