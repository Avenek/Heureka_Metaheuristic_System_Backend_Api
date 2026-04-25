using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class Algorithm : IEntity
    {
        public uint Id { get; set; }
        public string Name { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public bool IsRemoveable { get; set; }

        public ICollection<AlgorithmParameter> Parameters { get; set; } = new List<AlgorithmParameter>();
        public ICollection<SessionTest> SessionTests { get; set; } = new List<SessionTest>();
    }
}
