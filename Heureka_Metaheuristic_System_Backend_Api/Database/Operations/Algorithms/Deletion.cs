using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms
{
    public static partial class AlgorithmDatabaseOperations
    {
        private async static Task<bool> DeleteAlgorithm(
            this DatabaseOperationExecutionService service,
            AlgorithmDto algorithmDtoToDelete
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var algorithmToDelete = mapper.Map<Algorithm>(algorithmDtoToDelete);
            var result = await repositoryCollection.DeleteEntity(algorithmToDelete);
            return result;
        }

        public async static Task PerformDeleteAlgorithmOperations(
           this DatabaseOperationExecutionService service,
            AlgorithmDto algorithmToDelete
        )
        {
            if (algorithmToDelete is null)
            {
                throw new DatabaseOperationException($"{nameof(algorithmToDelete)} cannot be null.");
            }

            Func<Task<bool>>[] operations =
            [
                async () => await service.DeleteAlgorithm(algorithmToDelete),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
