using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class Session : IEntity, IHasId
    {
        public uint Id { get; set; }

        public uint StateId { get; set; }
        public SessionState State { get; set; } = null!;

        public ICollection<SessionTest> Tests { get; set; } = new List<SessionTest>();
    }
}
