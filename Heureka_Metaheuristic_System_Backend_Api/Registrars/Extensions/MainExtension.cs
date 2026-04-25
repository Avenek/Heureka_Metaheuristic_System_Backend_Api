using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.Middleware;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions
{
    public static class MainExtension
    {
        public static void AddMainRegistars(this IServiceCollection services)
        {
            services.AddScoped<ErrorHandlingMiddleware>();
            services.AddSingleton<IMappingService, DataDtoMappingService>();
            services.AddHttpContextAccessor();
        }
    }
}
