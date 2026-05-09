using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions
{
    public class CreateSessionDto
    {
        public required uint[] AlgorithmIds { get; set; }
        public required uint[] FitnessFunctionIds { get; set; }
        public required Dictionary<uint, List<AlgorithmParameterDto>> OverrideParametersConfig { get; set; }
        public uint NumberOfRunsPerParameterSet { get; set; }
    }
}
