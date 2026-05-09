namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions
{
    public class SessionTestResultsDto
    {
        public int SessionId { get; set; }
        public int AlgorithmId { get; set; }
        public int FitnessFunctionId { get; set; }
        public Dictionary<string, double> BestParams { get; set; }
        public double FBest { get; set; }
        public double[] XBest { get; set; }
        public int NumberOfEvaluationFitnessFunction { get; set; }
    }
}
