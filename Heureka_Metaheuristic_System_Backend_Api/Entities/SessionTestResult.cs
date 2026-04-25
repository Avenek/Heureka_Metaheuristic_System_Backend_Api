using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class SessionTestResult : IEntity
    {
        public uint Id { get; set; }

        public uint SessionTestId { get; set; }

        public string XBest { get; set; } = null!;
        public float FBest { get; set; }

        public uint FitnessFunctionEvaluations { get; set; }

        public string ParametersGrid { get; set; } = "{}";

        public SessionTest SessionTest { get; set; } = null!;
    }
}
