using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms
{
    public class CreateAlgorithmDto : IAlgorithmNameDto
    {
        public required string Name { get; set; }
    }
}
