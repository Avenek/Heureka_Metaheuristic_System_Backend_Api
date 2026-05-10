using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class SingleSessionTestData
    {
        public uint SessionId { get; init; }
        public uint AlgorithmId { get; init; }
        public required object AlgorithmInstance { get; init; }
        public uint FitnessFunctionId { get; init; }
        public required object FitnessFunctionInstance { get; init; }
        public uint NumberOfRunsPerParameterSet { get; init; }
        public required List<AlgorithmParameterDto> AlgorithmParametersConfig { get; set; }
    }
}
