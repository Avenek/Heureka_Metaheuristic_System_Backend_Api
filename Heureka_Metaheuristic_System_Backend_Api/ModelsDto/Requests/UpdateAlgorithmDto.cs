using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests
{
    public class UpdateAlgorithmDto : IAlgorithmNameDto
    {
        public required string Name { get; set; }
    }
}
