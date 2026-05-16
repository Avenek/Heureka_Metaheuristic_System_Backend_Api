using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;

namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class SingleSessionTestData
    {
        public uint SessionId { get; init; }
        public uint TestId { get; set; }
        public uint AlgorithmId { get; init; }
        public required ReflectionOptimizationAlgorithmAdapter AlgorithmInstance { get; init; }
        public required FitnessFunctionDto FitnessFunction { get; init; }
        public required ReflectionFitnessFunctionAdapter FitnessFunctionInstance { get; init; }
        public uint NumberOfRunsPerParameterSet { get; init; }
        public required List<AlgorithmParameterDto> AlgorithmParametersConfig { get; set; }
    }
}
