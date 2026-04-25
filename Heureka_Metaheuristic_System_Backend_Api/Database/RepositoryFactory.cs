using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class RepositoryFactory(IServiceProvider serviceProvider) : IRepositoryFactory
    {
        public DatabaseRepository<TEntity>? Create<TEntity>(DatabaseContext context)
            where TEntity : class, IEntity
            => (DatabaseRepository<TEntity>?)Activator.CreateInstance(typeof(DatabaseRepository<TEntity>), context);
    }

}
