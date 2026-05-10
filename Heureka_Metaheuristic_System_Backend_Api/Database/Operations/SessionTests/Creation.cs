using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTests
{
    public static partial class SessionTestDatabaseOperations
    {
        private async static Task<bool> CreateSessionTests(
            this DatabaseOperationExecutionService service,
            IEnumerable<SessionTest> sessionTestsToCreate
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.CreateEntities(sessionTestsToCreate);
            return result;
        }

        public async static Task<bool> PerformCreateSessionTestsOperations(
           this DatabaseOperationExecutionService service,
            IEnumerable<SessionTest> sessionTestsToCreate
        )
        {
            if (sessionTestsToCreate is null || sessionTestsToCreate.Count() == 0)
            {
                throw new DatabaseOperationException($"{nameof(sessionTestsToCreate)} cannot be null or empty.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.CreateSessionTests(sessionTestsToCreate),
            ];

            try
            {
                await service.CommitAllOrRollbackAsync(operations: operations);
                return true;
            }
            catch (Exception ex)
            {
                service.Logger.LogError(ex, $"An error occurred while creating sessionTests.");
                return false;
            }
        }
    }
}
