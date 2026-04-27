using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters
{
    public class ReflectionOptimizationAlgorithmAdapter : AbstractReflectionAdapter, IOptimizationAlgorithm
    {
        private readonly MethodInfo _solveMethod;

        public ReflectionOptimizationAlgorithmAdapter(object instance) : base(instance)
        {
            _solveMethod = Type.GetMethod("Solve")
                ?? throw new Exception("Solve method not found");
        }

        private T GetProperty<T>(string name)
        {
            var prop = Type.GetProperty(name)
                ?? throw new Exception($"Property {name} not found");

            return (T)prop.GetValue(Instance)!;
        }

        private void SetProperty<T>(string name, T value)
        {
            var prop = Type.GetProperty(name)
                ?? throw new Exception($"Property {name} not found");

            prop.SetValue(Instance, value);
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
                    ?? throw new Exception("ParamsInfo property not found");

                var array = (Array)prop.GetValue(Instance)!;

                return array
                    .Cast<object>()
                    .Select(x => new ReflectionParamInfoAdapter(x))
                    .ToArray();
            }
            set
            {
                throw new NotSupportedException("Setting ParamsInfo not supported for external plugins.");
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

        public int NumberOfEvaluationFitnessFunction
        {
            get => GetProperty<int>(nameof(NumberOfEvaluationFitnessFunction));
            set => SetProperty(nameof(NumberOfEvaluationFitnessFunction), value);
        }

        public void Solve(IFitnessFunction function, double[,] domain, double[] parameters, bool resume)
        {
            _solveMethod.Invoke(Instance, new object[]
            {
            function,
            domain,
            parameters,
            resume
            });
        }
    }
}
