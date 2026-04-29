using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions
{
    public static partial class FitnessFunctionDatabaseOperations
    {
        public static async Task<IEnumerable<FitnessFunctionDto>> GetAllFitnessFunctions(this DatabaseOperationExecutionService service)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var fitnessFunctions = await service.GetEntitiesBy<FitnessFunction>(f => true, GetFitnessFunctionDtoSelector()).ToListAsync();

            return mapper.Map<IEnumerable<FitnessFunctionDto>>(fitnessFunctions);
        }

        private static Expression<Func<FitnessFunction, FitnessFunction>> GetFitnessFunctionDtoSelector() => f => new FitnessFunction() { Id = f.Id, Name = f.Name, FileName = f.FileName, Dimension = f.Dimension, DomainPerVariable = f.DomainPerVariable, IsRemoveable = f.IsRemoveable };
    }
}
