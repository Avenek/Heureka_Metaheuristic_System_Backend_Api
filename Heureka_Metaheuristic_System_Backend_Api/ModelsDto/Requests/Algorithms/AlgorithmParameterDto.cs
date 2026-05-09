using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms
{
    public class AlgorithmParameterDto
    {
        public uint Id { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
}
