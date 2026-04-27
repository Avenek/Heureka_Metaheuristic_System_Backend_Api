using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic
{
    public static partial class GenericDatabaseQueryCollection
    {
        public static IQueryable<T> GetEntitiesBy<T>(this RepositoryCollection repositoryCollection,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, T>>? selector = null) where T : class, IEntity
        {
            var entities = repositoryCollection.Get<T>().Query();

            var whereQueryable = predicate is null ? entities : entities.Where(predicate);
            var selectQueryable = selector is null ? whereQueryable : whereQueryable.Select(selector);

            return selectQueryable;
        }
    }
}
