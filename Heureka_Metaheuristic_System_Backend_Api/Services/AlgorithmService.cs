using AutoMapper;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Users;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithms;
using Microsoft.AspNetCore.Identity;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface IAlgorithmService
    {
        IEnumerable<AlgorithmDto> GetAll();
    }

    public class AlgorithmService : IAlgorithmService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;

        public AlgorithmService(Func<DatabaseOperationExecutionService> executionServiceFactory)
        {
            this.executionServiceFactory = executionServiceFactory;
        }

        public IEnumerable<AlgorithmDto> GetAll()
        {
            var executionService = executionServiceFactory();
            var algorithmDtos = executionService.GetAllAlgorithms();
            return algorithmDtos;
        }
    }
}
