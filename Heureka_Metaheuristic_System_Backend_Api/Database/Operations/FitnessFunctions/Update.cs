using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions
{
    public static partial class FitnessFunctionDatabaseOperations
    {
        private async static Task<bool> UpdateFitnessFunction(
            this DatabaseOperationExecutionService service,
            UpdateFitnessFunctionDto updatedFitnessFunction, uint fitnessFunctionId
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var existingFitnessFunction = await repositoryCollection.Get<FitnessFunction>().GetById(fitnessFunctionId);

            var fitnessFunctionEntity = mapper.Map(updatedFitnessFunction, existingFitnessFunction);
            var result = await repositoryCollection.UpdateEntity(fitnessFunctionEntity);
            return result;
        }

        public async static Task PerformUpdateFitnessFunctionOperations(
           this DatabaseOperationExecutionService service,
            UpdateFitnessFunctionDto updatedFitnessFunction, uint fitnessFunctionId
        )
        {
            if (updatedFitnessFunction is null)
            {
                throw new DatabaseOperationException($"{nameof(updatedFitnessFunction)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.UpdateFitnessFunction(updatedFitnessFunction, fitnessFunctionId),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
