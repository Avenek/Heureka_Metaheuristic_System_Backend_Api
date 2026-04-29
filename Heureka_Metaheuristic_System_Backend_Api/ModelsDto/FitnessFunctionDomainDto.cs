using System.Text.Json.Serialization;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto
{
    public class FitnessFunctionDomainDto
    {
        [JsonPropertyName("min")]
        public double? Minimum { get; set; }
        [JsonPropertyName("max")]
        public double? Maximum { get; set; }
    }
}
