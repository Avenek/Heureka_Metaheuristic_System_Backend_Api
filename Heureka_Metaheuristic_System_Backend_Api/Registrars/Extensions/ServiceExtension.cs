using Heureka_Metaheuristic_System_Backend_Api.Services;
using System.Diagnostics;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var baseNamespace = typeof(AlgorithmService).Namespace;

            var serviceTypes = assembly.GetTypes().Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.Namespace != null &&
                type.Namespace.StartsWith(baseNamespace) &&
                type.GetInterfaces().Any(i =>
                    i.Namespace != null &&
                    i.Namespace.StartsWith(baseNamespace)));

            foreach (var serviceType in serviceTypes)
            {
                var interfaceType = serviceType.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, serviceType);
                }
            }
        }
    }
}
