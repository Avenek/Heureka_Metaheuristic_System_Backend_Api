namespace Heureka_Metaheuristic_System_Backend_Api.DataClasses
{
    public class TestIterationResults
    {
        public double[] XBest { get; set; }
        public double FBest { get; set; }
        public double Dimension { get; set; }
        public double[] ParameterValues { get; set; }
        public uint FitnessFunctionEvaluations { get; set; }
    }
}
