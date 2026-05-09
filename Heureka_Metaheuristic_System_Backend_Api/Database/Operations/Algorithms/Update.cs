using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms
{
    public static partial class AlgorithmDatabaseOperations
    {
        private async static Task<bool> UpdateAlgorithm(
            this DatabaseOperationExecutionService service,
            UpdateAlgorithmDto updatedAlgorithm, uint algorithmId
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var existingAlgorithm = await repositoryCollection.Get<Algorithm>().GetById(algorithmId);

            var algorithmEntity = mapper.Map(updatedAlgorithm, existingAlgorithm);
            var result = await repositoryCollection.UpdateEntity(algorithmEntity);
            return result;
        }

        public async static Task PerformUpdateAlgorithmOperations(
           this DatabaseOperationExecutionService service,
            UpdateAlgorithmDto updatedAlgorithm, uint algorithmId
        )
        {
            if (updatedAlgorithm is null)
            {
                throw new DatabaseOperationException($"{nameof(updatedAlgorithm)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.UpdateAlgorithm(updatedAlgorithm, algorithmId),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
