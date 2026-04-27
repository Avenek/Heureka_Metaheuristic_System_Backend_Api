using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class SessionTest : IEntity, IHasId
    {
        public uint Id { get; set; }

        public uint SessionId { get; set; }
        public uint AlgorithmId { get; set; }
        public uint FitnessFunctionId { get; set; }

        public uint TestInvokePerParameters { get; set; }

        public string ParametersConfig { get; set; } = "{}";
        public float Progress { get; set; }

        public Session Session { get; set; } = null!;
        public Algorithm Algorithm { get; set; } = null!;
        public FitnessFunction FitnessFunction { get; set; } = null!;

        public ICollection<SessionTestResult> Results { get; set; } = new List<SessionTestResult>();
    }
}
