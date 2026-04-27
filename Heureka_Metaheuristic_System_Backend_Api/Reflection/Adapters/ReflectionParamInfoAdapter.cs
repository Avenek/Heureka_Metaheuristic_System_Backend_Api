using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters
{
    public class ReflectionParamInfoAdapter : AbstractReflectionAdapter, IParamInfo
    {
        public ReflectionParamInfoAdapter(object instance) : base(instance)
        {
        }

        public string Name => GetProperty<string>(nameof(Name));

        public string Description => GetProperty<string>(nameof(Description));

        public double LowerBoundary
        {
            get => GetProperty<double>(nameof(LowerBoundary));
            set => SetProperty(nameof(LowerBoundary), value);
        }

        public double UpperBoundary
        {
            get => GetProperty<double>(nameof(UpperBoundary));
            set => SetProperty(nameof(UpperBoundary), value);
        }
    }
}
