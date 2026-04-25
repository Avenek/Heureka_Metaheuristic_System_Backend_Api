using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic
{
    public static partial class GenericDatabaseQueryCollection
    {
        public static IEnumerable<T> GetEntitiesBy<T>(this RepositoryCollection repositoryCollection,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, T>>? selector = null) where T : class, IEntity
        {
            var avatars = repositoryCollection.Get<T>().Query();

            var whereQueryable = predicate is null ? avatars : avatars.Where(predicate);
            var selectQueryable = selector is null ? whereQueryable : whereQueryable.Select(selector);

            return selectQueryable;
        }
    }
}
