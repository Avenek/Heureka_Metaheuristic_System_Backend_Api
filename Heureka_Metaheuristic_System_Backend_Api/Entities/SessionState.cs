using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Entities
{
    public class SessionState : IEntity
    {
        public uint Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
