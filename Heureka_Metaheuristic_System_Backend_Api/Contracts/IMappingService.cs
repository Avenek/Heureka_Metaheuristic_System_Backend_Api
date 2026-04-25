using AutoMapper;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts
{
    public interface IMappingService : IService
    {
        IMapper Mapper { get; }
    }
}
