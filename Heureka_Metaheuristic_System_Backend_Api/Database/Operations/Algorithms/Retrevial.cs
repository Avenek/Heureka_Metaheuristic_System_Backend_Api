using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Users;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Users
{
    public static partial class AlgorithmDatabaseOperations
    {
        public static IEnumerable<AlgorithmDto> GetAllAlgorithms(this DatabaseOperationExecutionService service)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var algorithms = service.GetEntitiesBy<Algorithm>(a => true, a => new Algorithm() { Id = a.Id, Name = a.Name, IsRemoveable = a.IsRemoveable });

            return mapper.Map<IEnumerable<AlgorithmDto>>(algorithms);
        }
    }
}
