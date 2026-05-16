using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters
{
    public class ReflectionOptimizationAlgorithmAdapter : AbstractReflectionAdapter, IOptimizationAlgorithm
    {
        private readonly MethodInfo solveMethod;

        public ReflectionOptimizationAlgorithmAdapter(object instance) : base(instance)
        {
            solveMethod = Type.GetMethod(nameof(Solve))
                ?? throw new Exception($"{nameof(Solve)} method not found");
        }

        public string Name
        {
            get => GetProperty<string>(nameof(Name));
            set => SetProperty(nameof(Name), value);
        }

        public IParamInfo[] ParamsInfo
        {
            get
            {
                var prop = Type.GetProperty(nameof(ParamsInfo))
                    ?? throw new Exception($"{nameof(ParamsInfo)} property not found");

                var array = (Array)prop.GetValue(Instance)!;

                return array
                    .Cast<object>()
                    .Select(x => new ReflectionParamInfoAdapter(x))
                    .ToArray();
            }
            set
            {
                throw new NotSupportedException($"Setting {nameof(ParamsInfo)} not supported for external plugins.");
            }
        }

        public double[] XBest
        {
            get => GetProperty<double[]>(nameof(XBest));
            set => SetProperty(nameof(XBest), value);
        }

        public double FBest
        {
            get => GetProperty<double>(nameof(FBest));
            set => SetProperty(nameof(FBest), value);
        }

        public uint NumberOfEvaluationFitnessFunction
        {
            get => GetProperty<uint>(nameof(NumberOfEvaluationFitnessFunction));
            set => SetProperty(nameof(NumberOfEvaluationFitnessFunction), value);
        }

        public void Solve(IFitnessFunction function, uint dimension, double[,] domain, double[] parameters)
        {
            solveMethod.Invoke(Instance, new object[]
            {
                function,
                dimension,
                domain,
                parameters
            });
        }
    }
}
