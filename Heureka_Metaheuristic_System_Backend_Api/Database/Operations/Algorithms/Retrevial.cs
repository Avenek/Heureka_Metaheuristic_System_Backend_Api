using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Extensions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms
{
    public static partial class AlgorithmDatabaseOperations
    {
        public static async Task<IEnumerable<AlgorithmDto>> GetAllAlgorithms(this DatabaseOperationExecutionService service)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var algorithms = await service.GetEntitiesBy<Algorithm>(a => true, GetAlgorithmDtoSelector()).ToListAsync();

            return mapper.Map<IEnumerable<AlgorithmDto>>(algorithms);
        }

        public static async Task<AlgorithmDto> GetAlgorithmById(this DatabaseOperationExecutionService service, uint id)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var algorithm = await service.GetEntitiesBy<Algorithm>(a => a.Id == id, GetAlgorithmDtoSelector())
                .SingleOrDefaultAsync();

            algorithm.ThrowIfNull($"Algorithm with id {id} not found.");

            return mapper.Map<AlgorithmDto>(algorithm);
        }

        private static Expression<Func<Algorithm, Algorithm>> GetAlgorithmDtoSelector() => a => new Algorithm() { Id = a.Id, Name = a.Name, FileName = a.FileName, IsRemoveable = a.IsRemoveable };
    }
}
