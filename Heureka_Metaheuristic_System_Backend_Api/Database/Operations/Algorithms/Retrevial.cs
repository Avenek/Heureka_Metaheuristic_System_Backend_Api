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

        public static async Task<AlgorithmDto> GetAlgorithmWithParametersById(this DatabaseOperationExecutionService service, uint id)
        {
            var algorithms = await service.GetAlgorithmWithParametersByIds([id]);
            return algorithms.SingleOrDefault().ThrowIfNull($"Algorithm with id {id} not found.");
        }

        public static async Task<List<AlgorithmDto>> GetAlgorithmWithParametersByIds(this DatabaseOperationExecutionService service, IEnumerable<uint> ids)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var algorithm = await service.GetEntitiesBy<Algorithm>(a => ids.Contains(a.Id),
                    a => new Algorithm()
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        Parameters = a.Parameters.Select(p => new AlgorithmParameter() { Id = p.Id, MinValue = p.MinValue, MaxValue = p.MaxValue }).ToList()
                    })
                .SingleOrDefaultAsync();

            return mapper.Map<List<AlgorithmDto>>(algorithm);
        }

        private static Expression<Func<Algorithm, Algorithm>> GetAlgorithmDtoSelector() => a => new Algorithm() { Id = a.Id, Name = a.Name, FileName = a.FileName, IsRemoveable = a.IsRemoveable };
    }
}
