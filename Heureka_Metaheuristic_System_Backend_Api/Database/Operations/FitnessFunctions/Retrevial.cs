using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Extensions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Core.Mapping;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions
{
    public static partial class FitnessFunctionDatabaseOperations
    {
        public static async Task<List<FitnessFunctionDto>> GetAllFitnessFunctions(this DatabaseOperationExecutionService service)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var fitnessFunctions = await service.GetEntitiesBy<FitnessFunction>(f => true, GetFitnessFunctionDtoSelector()).ToListAsync();

            return mapper.Map<List<FitnessFunctionDto>>(fitnessFunctions);
        }

        public static async Task<FitnessFunctionDto> GetFitnessFunctionById(this DatabaseOperationExecutionService service, uint id)
        {
            var fitnessFunctions = await service.GetFitnessFunctionByIds([id]);
            return fitnessFunctions.SingleOrDefault().ThrowIfNull($"Fitness function with id {id} was not found.");
        }

        public static async Task<List<FitnessFunctionDto>> GetFitnessFunctionByIds(this DatabaseOperationExecutionService service, IEnumerable<uint> ids)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var fitnessFunction = await service.GetEntitiesBy<FitnessFunction>(a => ids.Contains(a.Id), GetFitnessFunctionDtoSelector()).ToListAsync();

            return mapper.Map<List<FitnessFunctionDto>>(fitnessFunction);
        }

        private static Expression<Func<FitnessFunction, FitnessFunction>> GetFitnessFunctionDtoSelector() => f => new FitnessFunction() { Id = f.Id, Name = f.Name, FileName = f.FileName, Dimension = f.Dimension, DomainPerVariable = f.DomainPerVariable, IsRemoveable = f.IsRemoveable };
    }
}
