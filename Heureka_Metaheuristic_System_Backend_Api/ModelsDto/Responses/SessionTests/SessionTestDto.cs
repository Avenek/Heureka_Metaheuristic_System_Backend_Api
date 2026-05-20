using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.SessionTests
{
    [AutoMap(typeof(SessionTest), ReverseMap = true)]
    public class SessionTestDto
    {
        public uint Id { get; set; }
        public uint AlgorithmId { get; set; }
        public uint FitnessFunctionId { get; set; }
        public double Progress { get; set; }
    }
}
