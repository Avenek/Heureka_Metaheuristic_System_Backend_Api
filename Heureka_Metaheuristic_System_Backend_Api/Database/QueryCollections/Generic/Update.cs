using Heureka_Metaheuristic_System_Backend_Api.Contracts;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic
{
    public static partial class GenericDatabaseQueryCollection
    {
        public async static Task<bool> UpdateEntity<T>(
              this RepositoryCollection repositoryCollection,
              T updatedEntity
         ) where T : class, IEntity, IHasId
        {
            var context = repositoryCollection.Context;
            var entityRepository = repositoryCollection.Get<T>();
            var existingEntity = await entityRepository.GetById(updatedEntity.Id);
            if (existingEntity is null)
            {
                return false;
            }

            context.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

            return await context.SaveChangesAsync() > 0;
        }
    }
}
