using Heureka_Metaheuristic_System_Backend_Api.Database;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts
{
    public interface IRepositoryFactory
    {
        DatabaseRepository<TEntity>? Create<TEntity>(DatabaseContext context)
            where TEntity : class, IEntity;
    }
}
