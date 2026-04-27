namespace Heureka_Metaheuristic_System_Backend_Api.Configuration
{
    public class AppSettings
    {
        public LoggingConfiguration Logging { get; set; } = new();
        public DllPathsConfiguration DllPaths { get; set; } = new();
    }
}
