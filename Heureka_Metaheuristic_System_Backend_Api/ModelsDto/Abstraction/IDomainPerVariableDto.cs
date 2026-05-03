namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction
{
    public interface IDomainPerVariableDto
    {
        List<FitnessFunctionDomainDto> DomainPerVariable { get; }
    }
}
