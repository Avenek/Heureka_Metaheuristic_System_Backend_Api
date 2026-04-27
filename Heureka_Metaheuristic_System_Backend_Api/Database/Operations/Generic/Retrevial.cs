using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic
{
    public static partial class GenericDatabaseOperations
    {
        public static IQueryable<T> GetEntitiesBy<T>(this DatabaseOperationExecutionService service, 
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, T>>? selector = null) where T : class, IEntity
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var entities = repositoryCollection.GetEntitiesBy(predicate, selector);

            return entities;
        }
    }
}
