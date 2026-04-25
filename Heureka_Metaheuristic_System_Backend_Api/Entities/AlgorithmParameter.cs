using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class AlgorithmParameter : IEntity
    {
        public uint Id { get; set; }
        public string Name { get; set; } = null!;

        public uint AlgorithmId { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public Algorithm Algorithm { get; set; } = null!;
    }
}
