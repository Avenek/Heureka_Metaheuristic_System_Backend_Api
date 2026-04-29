using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss
{
    [AutoMap(typeof(Algorithm), ReverseMap = true)]
    public class AlgorithmDto
    {
        public uint Id { get; set; }
        public required string Name { get; set; }
        public required string FileName { get; set; }
        public bool IsRemoveable { get; set; }
    }
}
