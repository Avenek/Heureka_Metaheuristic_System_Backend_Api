using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions
{
    public static partial class SessionDatabaseOperations
    {
        private async static Task<bool> CreateSession(
            this DatabaseOperationExecutionService service,
            Session sessionToCreate
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.CreateEntity(sessionToCreate);
            return result;
        }

        public async static Task<bool> PerformCreateSessionOperations(
           this DatabaseOperationExecutionService service,
            Session sessionToCreate
        )
        {
            if (sessionToCreate is null)
            {
                throw new DatabaseOperationException($"{nameof(sessionToCreate)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.CreateSession(sessionToCreate),
            ];

            try
            {
                await service.CommitAllOrRollbackAsync(operations: operations);
                return true;
            }
            catch (Exception ex)
            {
                service.Logger.LogError(ex, $"An error occurred while creating session.");
                return false;
            }
        }
    }
}
