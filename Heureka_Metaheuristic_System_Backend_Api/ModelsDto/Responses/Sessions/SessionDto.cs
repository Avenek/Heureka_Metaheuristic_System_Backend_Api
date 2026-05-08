namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions
{
    public class SessionDto
    {
        public uint Id { get; set; }
        public uint State { get; set; }
        public List<uint> AlgorithmIds { get; set; } = new();
        public List<uint> FitnessFunctionIds { get; set; } = new();
    }
}
