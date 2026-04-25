using Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars
{
    public class Registar
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            AddServices(services, assembly);
        }

        public static void AddServices(IServiceCollection services, Assembly assembly)
        {
            services.AddServicesFromAssembly(assembly);
            services.AddMappersFromAssembly(assembly);
            services.AddMainRegistars();
            services.AddDatabaseRegistars();
        }
    }
}
