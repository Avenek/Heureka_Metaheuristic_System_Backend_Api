namespace Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic
{
    public interface IDatabaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity;
}
