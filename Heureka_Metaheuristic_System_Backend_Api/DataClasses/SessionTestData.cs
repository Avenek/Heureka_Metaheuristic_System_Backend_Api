namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class SessionTestData
    {
        public uint SessionId { get; init; }
        public required Dictionary<uint, Type> AlgorithmTypes { get; init; }
        public required Dictionary<uint, Type> FitnessFunctionsTypes { get; init;  }
        public uint NumberOfRunsPerParameterSet { get; init; }
        public CancellationToken CancellationToken { get; init; }
    }
}
