using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.SessionTests;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions
{
    public class SessionWithTestsDto
    {
        public uint Id { get; set; }
        public uint State { get; set; }
        public List<SessionTestDto> Tests { get; set; } = new();
    }
}
