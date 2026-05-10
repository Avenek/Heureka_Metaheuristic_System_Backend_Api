using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class SessionTestsData
    {
        public uint SessionId { get; init; }
        public required Dictionary<uint, object> AlgorithmInstances { get; init; }
        public required Dictionary<uint, object> FitnessFunctionsInstances { get; init;  }
        public uint NumberOfRunsPerParameterSet { get; init; }
        public required Dictionary<uint, List<AlgorithmParameterDto>> AlgorithmParametersConfig { get; set; }
        public Dictionary<uint, List<AlgorithmParameterDto>> OverrideAlgorithmParametersConfig { get; set; } = new();
        public CancellationToken CancellationToken { get; init; }
    }
}
