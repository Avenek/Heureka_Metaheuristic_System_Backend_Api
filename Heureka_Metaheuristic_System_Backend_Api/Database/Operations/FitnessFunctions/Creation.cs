using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions
{
    public static partial class FitnessFunctionDatabaseOperations
    {
        private async static Task<bool> CreateFitnessFunction(
            this DatabaseOperationExecutionService service,
            FitnessFunction fitnessFunctionToCreate
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var result = await repositoryCollection.CreateEntity(fitnessFunctionToCreate);
            return result;
        }

        public async static Task<bool> PerformCreateFitnessFunctionOperations(
           this DatabaseOperationExecutionService service,
            FitnessFunction fitnessFunctionToCreate
        )
        {
            if (fitnessFunctionToCreate is null)
            {
                throw new DatabaseOperationException($"{nameof(fitnessFunctionToCreate)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.CreateFitnessFunction(fitnessFunctionToCreate),
            ];

            try
            {
                await service.CommitAllOrRollbackAsync(operations: operations);
                return true;
            }
            catch (Exception ex)
            {
                service.Logger.LogError(ex, $"An error occurred while creating fitnessFunction with name {fitnessFunctionToCreate.Name}.");
                return false;
            }
        }
    }
}
