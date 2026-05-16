using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.SessionTests;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTests
{
    public static partial class SessionTestDatabaseOperations
    {
        private async static Task<bool> UpdateSessionTestProgress(
            this DatabaseOperationExecutionService service,
            uint sessionTestId, double newProgress
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.UpdateSessionTestProgress(sessionTestId, newProgress);
            return result;
        }

        public async static Task PerformUpdateSessionTestProgressOperations(
           this DatabaseOperationExecutionService service,
            uint sessionTestId, double newProgress
        )
        {
            Func<Task<bool>>[] operations =
            [
                async () => await service.UpdateSessionTestProgress(sessionTestId, newProgress),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
