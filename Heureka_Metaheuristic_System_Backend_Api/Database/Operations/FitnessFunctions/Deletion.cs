using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions
{
    public static partial class FitnessFunctionDatabaseOperations
    {
        private async static Task<bool> DeleteFitnessFunction(
            this DatabaseOperationExecutionService service,
            FitnessFunctionDto fitnessFunctionDtoToDelete
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var fitnessFunctionToDelete = mapper.Map<FitnessFunction>(fitnessFunctionDtoToDelete);
            var result = await repositoryCollection.DeleteEntity(fitnessFunctionToDelete);
            return result;
        }

        public async static Task PerformDeleteFitnessFunctionOperations(
           this DatabaseOperationExecutionService service,
            FitnessFunctionDto fitnessFunctionToDelete
        )
        {
            if (fitnessFunctionToDelete is null)
            {
                throw new DatabaseOperationException($"{nameof(fitnessFunctionToDelete)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.DeleteFitnessFunction(fitnessFunctionToDelete),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
