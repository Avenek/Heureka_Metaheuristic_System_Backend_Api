namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters
{
    public abstract class AbstractReflectionAdapter
    {
        protected readonly object Instance;
        protected readonly Type Type;
        protected AbstractReflectionAdapter(object instance)
        {
            Instance = instance;
            Type = instance.GetType();
        }
        protected T GetProperty<T>(string name)
        {
            var prop = Type.GetProperty(name)
                ?? throw new Exception($"Property {name} not found");

            return (T)prop.GetValue(Instance)!;
        }

        protected void SetProperty<T>(string name, T value)
        {
            var prop = Type.GetProperty(name)
                ?? throw new Exception($"Property {name} not found");

            prop.SetValue(Instance, value);
        }
    }
}
