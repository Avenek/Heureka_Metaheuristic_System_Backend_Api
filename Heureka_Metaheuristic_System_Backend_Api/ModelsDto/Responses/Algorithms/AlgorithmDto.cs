using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithms
{
    [AutoMap(typeof(Algorithm), ReverseMap = true)]
    public class AlgorithmDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsRemoveable { get; set; }
    }
}
