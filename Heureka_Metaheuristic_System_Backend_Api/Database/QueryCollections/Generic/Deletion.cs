using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic
{
    public static partial class GenericDatabaseQueryCollection
    {
        public async static Task<bool> DeleteEntity<T>(
            this RepositoryCollection repositoryCollection,
            uint id
        ) where T : class, IEntity, IHasId
        {
            var context = repositoryCollection.Context;
            var entityRepository = repositoryCollection.Get<T>();
            var entity = entityRepository.GetById(id);

            context.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }
        public async static Task<bool> DeleteEntity<T>(
            this RepositoryCollection repositoryCollection,
            T entity
        ) where T : class, IEntity, IHasId
        {
            var context = repositoryCollection.Context;
            var entityRepository = repositoryCollection.Get<T>();

            context.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }
    }
}
