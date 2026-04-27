namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces
{
    public interface IOptimizationAlgorithm
    {
        string Name { get; set; }

        void Solve(IFitnessFunction function, double[,] domain, double[] parameters, bool resume);

        IParamInfo[] ParamsInfo { get; set; }

        double[] XBest { get; set; }
        double FBest { get; set; }
        int NumberOfEvaluationFitnessFunction { get; set; }
    }
}
