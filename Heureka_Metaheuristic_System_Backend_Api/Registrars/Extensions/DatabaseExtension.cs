using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Middleware;
using Microsoft.AspNetCore.Identity;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions
{
    public static class DatabaseExtension
    {
        public static void AddDatabaseRegistars(this IServiceCollection services)
        {
            services.AddTransient<IRepositoryCollection, RepositoryCollection>();
            services.AddTransient<IRepositoryFactory, RepositoryFactory>();
            services.AddSingleton(typeof(IEntityPropertyRetriever<>), typeof(PropertyRetriever<>));
            services.AddTransient<DatabaseOperationExecutionService>();
            services.AddTransient<Func<DatabaseOperationExecutionService>>(provider =>
            {
                return () => provider.GetRequiredService<DatabaseOperationExecutionService>();
            });
        }
    }
}
