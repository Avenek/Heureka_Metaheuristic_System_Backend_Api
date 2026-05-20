using AutoMapper;
using DotNetEnv;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTestResults;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTests;
using Heureka_Metaheuristic_System_Backend_Api.DataClasses;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Enums;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;
using Sprache;
using System.Text.Json;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionDto>> GetAll(uint? stateId);
        Task DeleteById(uint id);
        Task<SessionDto> CreateSession(CreateSessionDto createSessionDto);
        Task ResumeSession(uint id, CancellationToken cancellationToken);
    }

    public class SessionService : ISessionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly SessionTestRunner sessionTestRunner;

        public SessionService(Func<DatabaseOperationExecutionService> executionServiceFactory, SessionTestRunner sessionTestRunner)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.sessionTestRunner = sessionTestRunner;
        }

        public async Task<IEnumerable<SessionDto>> GetAll(uint? stateId)
        {
            var executionService = executionServiceFactory();
            var sessionDtos = await executionService.GetAllSessions(stateId);
            return sessionDtos;
        }

        public async Task DeleteById(uint id)
        {
            var executionService = executionServiceFactory();
            await executionService.PerformDeleteSessionOperations(id);
        }

        public async Task<SessionDto> CreateSession(CreateSessionDto createSessionDto)
        {
            var executionService = executionServiceFactory();
            var session = new Session
            {
                StateId = (uint)ESessionState.Suspended
            };

            await executionService.PerformCreateSessionOperations(session);

            var sessionTestData = await BuildSessionData(executionService, createSessionDto);

            var testCombinations = sessionTestRunner.CreateSessionTestCombinations(sessionTestData, executionService);

            await CreateSessionTestsInDatabase(executionService, testCombinations);

            var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
            return mapper.Map<SessionDto>(session);
        }

        public async Task ResumeSession(uint id, CancellationToken cancellationToken)
        {
            await sessionTestRunner.ResumeSessionAsync(id, cancellationToken);
        }

        private async Task<SessionTestsData> BuildSessionData(DatabaseOperationExecutionService executionService, CreateSessionDto createSessionDto)
        {
            var algorithms = await executionService.GetAlgorithmWithParametersByIds(createSessionDto.AlgorithmIds);
            var fitnessFunctions = await executionService.GetFitnessFunctionForTestByIds(createSessionDto.FitnessFunctionIds);

            if (algorithms.Count != createSessionDto.AlgorithmIds.Length)
                throw new NotFoundException("One or more algorithms were not found.");

            if (fitnessFunctions.Count != createSessionDto.FitnessFunctionIds.Length)
                throw new NotFoundException("One or more fitness functions were not found.");

            return new SessionTestsData
            {
                Algorithms = algorithms.ToDictionary(a => a.Id, a => a),
                FitnessFunctions = fitnessFunctions.ToDictionary(f => f.Id, f => f),
                NumberOfRunsPerParameterSet = createSessionDto.NumberOfRunsPerParameterSet,
                OverrideAlgorithmParametersConfig = createSessionDto.OverrideParametersConfig
            };
        }

        private async Task CreateSessionTestsInDatabase(DatabaseOperationExecutionService executionService, List<SingleSessionTestRunner> tests)
        {
            var entities = tests.Select(t => new SessionTest
            {
                SessionId = t.SessionId,
                AlgorithmId = t.AlgorithmId,
                FitnessFunctionId = t.FitnessFunction.Id,
                TestInvokePerParameters = t.NumberOfRunsPerParameterSet,
                ParametersConfig = JsonSerializer.Serialize(t.AlgorithmParametersConfig),
                Progress = 0
            }).ToList();

            await executionService.PerformCreateSessionTestsOperations(entities);

            foreach (var t in tests)
            {
                t.TestId = entities.First(e =>
                    e.SessionId == t.SessionId &&
                    e.AlgorithmId == t.AlgorithmId &&
                    e.FitnessFunctionId == t.FitnessFunction.Id).Id;
            }
        }
    }
}
