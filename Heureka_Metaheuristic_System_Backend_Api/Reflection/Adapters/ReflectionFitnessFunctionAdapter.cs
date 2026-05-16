using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters
{
    public class ReflectionFitnessFunctionAdapter : AbstractReflectionAdapter, IFitnessFunction
    {
        private readonly MethodInfo calculateMethod;

        public ReflectionFitnessFunctionAdapter(object instance) : base(instance)
        {
            calculateMethod = Type.GetMethod(nameof(Calculate))
                ?? throw new Exception($"{nameof(Calculate)} method not found");
        }

        public double Calculate(double[] position)
        {
            return (double)calculateMethod.Invoke(Instance, new object[]
            {
                position
            });
        }
    }
}
