using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions
{
    public class CreateFitnessFunctionDto : INameDto
    {
        public required string Name { get; set; }
        public int? Dimension { get; set; }
        public List<FitnessFunctionDomainDto> DomainPerVariable { get; set; } = new();
    }
}
