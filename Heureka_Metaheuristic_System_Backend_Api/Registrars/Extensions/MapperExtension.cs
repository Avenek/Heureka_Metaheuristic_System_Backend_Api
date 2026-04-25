
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions
{
    public static class MapperExtension
    {
        public static void AddMappersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
