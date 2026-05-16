namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces
{
    public interface IOptimizationAlgorithm
    {
        string Name { get; set; }

        void Solve(IFitnessFunction function, uint dimension, double[,] domain, double[] parameters);

        IParamInfo[] ParamsInfo { get; set; }

        double[] XBest { get; set; }
        double FBest { get; set; }
        uint NumberOfEvaluationFitnessFunction { get; set; }
    }
}
