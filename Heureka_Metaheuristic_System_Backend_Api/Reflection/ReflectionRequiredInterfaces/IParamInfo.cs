namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces
{
    public interface IParamInfo
    {
        public string Name { get; }
        public string Description { get; }
        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }
    }
}
