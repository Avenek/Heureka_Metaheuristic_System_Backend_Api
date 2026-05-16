using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;

namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class SessionTestsData
    {
        public uint SessionId { get; init; }
        public required Dictionary<uint, Algorithm> Algorithms { get; init; }
        public required Dictionary<uint, FitnessFunction> FitnessFunctions { get; init;  }
        public uint NumberOfRunsPerParameterSet { get; init; }
        public Dictionary<uint, List<AlgorithmParameterDto>> OverrideAlgorithmParametersConfig { get; set; } = new();
        public CancellationToken CancellationToken { get; init; }
    }
}
