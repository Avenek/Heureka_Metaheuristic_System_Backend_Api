using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class AlgorithmParameter : IEntity, IHasId
    {
        public uint Id { get; set; }
        public string Name { get; set; } = null!;

        public uint AlgorithmId { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public Algorithm Algorithm { get; set; } = null!;
    }
}
