using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;

namespace Heureka_Metaheuristic_System_Backend_Api.Helpers
{
    public abstract class SingleSessionTestRunner
    {
        public uint SessionId { get; set; }
        public uint TestId { get; set; }
        public uint AlgorithmId { get; set; }
        public ReflectionOptimizationAlgorithmAdapter AlgorithmInstance { get; set; }
        public FitnessFunctionDto FitnessFunction { get; set; }
        public ReflectionFitnessFunctionAdapter FitnessFunctionInstance { get; set; }
        public uint NumberOfRunsPerParameterSet { get; set; }
        public List<AlgorithmParameterDto> AlgorithmParametersConfig { get; set; }

        public int InitialDimension { get; protected set; }
        public double[] InitialParameters { get; set; }
        public int MinimalDimension => FitnessFunction.Dimension ?? 2;

        public abstract Task PrepareInitializeParameters();
    }
}
