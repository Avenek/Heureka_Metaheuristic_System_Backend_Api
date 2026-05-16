using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTestResults
{
    public static partial class SessionTestResultDatabaseOperations
    {
        private async static Task<bool> CreateSessionTestResults(
            this DatabaseOperationExecutionService service,
            IEnumerable<SessionTestResult> sessionTestResultsToCreate
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.CreateEntities(sessionTestResultsToCreate);
            return result;
        }

        public async static Task PerformCreateSessionTestResultsOperations(
           this DatabaseOperationExecutionService service,
            IEnumerable<SessionTestResult> sessionTestResultsToCreate
        )
        {
            if (sessionTestResultsToCreate is null || sessionTestResultsToCreate.Count() == 0)
            {
                throw new DatabaseOperationException($"{nameof(sessionTestResultsToCreate)} cannot be null or empty.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.CreateSessionTestResults(sessionTestResultsToCreate),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
