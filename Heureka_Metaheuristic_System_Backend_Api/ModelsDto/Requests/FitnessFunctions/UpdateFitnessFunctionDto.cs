using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions
{
    public class UpdateFitnessFunctionDto : INameDto, IDomainPerVariableDto
    {
        public string Name { get; set; } = null!;
        public uint? Dimension { get; set; }
        public List<FitnessFunctionDomainDto>? DomainPerVariable { get; set; }
    }
}
