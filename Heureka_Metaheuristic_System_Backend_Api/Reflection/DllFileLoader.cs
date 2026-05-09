using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection
{
    public class DllFileLoader
    {
        private readonly AppSettings appSettings;

        public DllFileLoader(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Assembly Load(string path)
        {
            var assemblyLoadContext = new AssemblyLoadContext("TemporaryAssemblyLoadContext");
            var assemblyBytes = File.ReadAllBytes(path);
            return assemblyLoadContext.LoadFromStream(new MemoryStream(assemblyBytes));
        }

        public Assembly LoadFromStream(Stream stream)
        {
            var assemblyLoadContext = new AssemblyLoadContext("TemporaryAssemblyLoadContext");
            return assemblyLoadContext.LoadFromStream(stream);
        }

        public Type GetOptimizationType<T>(IFormFile file)
        {
            using var readStream = file.OpenReadStream();
            var assembly = LoadFromStream(readStream);
            return FindMatchingType<T>(assembly);
        }
        public Type GetOptimizationType<T>(string path)
        {
            var assembly = Load(path);
            return FindMatchingType<T>(assembly);
        }

        private Type FindMatchingType<T>(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var optimizationType = types.FirstOrDefault(type => type.GetInterfaces().Any(interfaceType => ReflectionValidator.ImplementsInterface(interfaceType, typeof(T))));
            if (optimizationType == null)
            {
                throw new BadRequestException($"File {assembly.FullName} does not implement the required interface.");
            }

            return optimizationType;
        }

        public string GetFitnessFunctionFilePath(string fileName)
        {
            return Path.Combine(appSettings.DllPaths.FunctionsPath, fileName);
        }
        public string GetAlgorithmFilePath(string fileName)
        {
            return Path.Combine(appSettings.DllPaths.AlgorithmsPath, fileName);
        }
    }
}
