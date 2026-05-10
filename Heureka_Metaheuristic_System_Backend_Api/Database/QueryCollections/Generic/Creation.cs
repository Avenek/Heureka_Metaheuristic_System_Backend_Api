using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic
{
    public static partial class GenericDatabaseQueryCollection
    {
        public async static Task<bool> CreateEntity<T>(
            this RepositoryCollection repositoryCollection,
            T entity
        ) where T : class, IEntity
        {
            var context = repositoryCollection.Context;
            await context.AddAsync(entity);

            return await context.SaveChangesAsync() > 0;
        }
        public async static Task<bool> CreateEntities<T>(
            this RepositoryCollection repositoryCollection,
            IEnumerable<T> entities
        ) where T : class, IEntity
        {
            var context = repositoryCollection.Context;
            await context.AddRangeAsync(entities);

            return await context.SaveChangesAsync() > 0;
        }
    }
}
