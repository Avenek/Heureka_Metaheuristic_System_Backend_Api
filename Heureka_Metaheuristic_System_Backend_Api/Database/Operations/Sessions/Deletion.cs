using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions
{
    public static partial class SessionDatabaseOperations
    {
        private async static Task<bool> DeleteSessionById(
            this DatabaseOperationExecutionService service,
            uint id
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.DeleteEntity<Session>(id);
            return result;
        }

        public async static Task PerformDeleteSessionOperations(
           this DatabaseOperationExecutionService service,
            uint id
        )
        {
            Func<Task<bool>>[] operations =
            [
                async () => await service.DeleteSessionById(id),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
