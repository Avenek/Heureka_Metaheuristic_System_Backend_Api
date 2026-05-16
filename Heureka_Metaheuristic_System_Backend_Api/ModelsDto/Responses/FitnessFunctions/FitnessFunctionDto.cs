namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions
{
    public class FitnessFunctionDto
    {
        public uint Id { get; set; }
        public required string Name { get; set; }
        public string? FileName { get; set; }
        public string? ClassName { get; set; }
        public int? Dimension { get; set; }
        public List<FitnessFunctionDomainDto> DomainPerVariable { get; set; } = new();
        public bool IsRemoveable { get; set; }
    }
}
