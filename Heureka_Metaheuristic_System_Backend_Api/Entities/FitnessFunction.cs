using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class FitnessFunction : IEntity, IHasId
    {
        public uint Id { get; set; }
        public string Name { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public bool IsRemoveable { get; set; }

        public byte? Dimension { get; set; }
        public string DomainPerVariable { get; set; } = "[]";

        public ICollection<SessionTest> SessionTests { get; set; } = new List<SessionTest>();
    }
}
