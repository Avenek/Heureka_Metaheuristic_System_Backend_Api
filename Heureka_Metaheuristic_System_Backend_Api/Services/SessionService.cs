using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTests;
using Heureka_Metaheuristic_System_Backend_Api.DataClasses;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Enums;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using System.Collections.Concurrent;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionDto>> GetAll(uint? stateId);
        Task DeleteById(uint id);
        Task<IEnumerable<SessionTestResultsDto>> CreateSession(CreateSessionDto createSessionDto, CancellationToken cancellationToken);
    }

    public class SessionService : ISessionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly DllFileLoader dllFileLoader;

        public SessionService(Func<DatabaseOperationExecutionService> executionServiceFactory, DllFileLoader dllFileLoader)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.dllFileLoader = dllFileLoader;
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

        public async Task<IEnumerable<SessionTestResultsDto>> CreateSession(CreateSessionDto createSessionDto, CancellationToken cancellationToken)
        {
            var executionService = executionServiceFactory();
            var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;

            await CancelIfAnySessionIsRunning(executionService);

            var createdSession = await CreateSessionInDatabase(executionService);

            var algorithmIds = createSessionDto.AlgorithmIds;
            var fitnessFunctionIds = createSessionDto.FitnessFunctionIds;

            if (algorithmIds.Length == 0 || fitnessFunctionIds.Length == 0)
            {
                throw new BadRequestException("You must provide at least one algorithm and one fitness function.");
            }

            var algorithms = await executionService.GetAlgorithmWithParametersByIds(algorithmIds);

            var fitnessFunctions = await executionService.GetFitnessFunctionForTestByIds(fitnessFunctionIds);

            if (algorithms.Count != algorithmIds.Length)
            {
                throw new NotFoundException("One or more algorithms were not found.");
            }
               
            if (fitnessFunctions.Count != fitnessFunctionIds.Length)
            {
                throw new NotFoundException("One or more fitness functions were not found.");
            }

            var algorithmInstances = algorithms.ToDictionary(
                a => a.Id,
                a => dllFileLoader.CreateInstance(
                    dllFileLoader.GetAlgorithmFilePath(a.FileName),
                    a.ClassName));

            var fitnessFunctionInstances = fitnessFunctions.ToDictionary(
                f => f.Id,
                f => dllFileLoader.CreateInstance(
                    dllFileLoader.GetFitnessFunctionFilePath(f.FileName),
                    f.ClassName));

            var algorithmParams = algorithms.ToDictionary(
                a => a.Id,
                a => mapper.Map<List<AlgorithmParameterDto>>(a.Parameters));

            var sessionTestData = new SessionTestsData
            {
                SessionId = createdSession.Id,
                AlgorithmInstances = algorithmInstances,
                FitnessFunctionsInstances = fitnessFunctionInstances,
                NumberOfRunsPerParameterSet = createSessionDto.NumberOfRunsPerParameterSet,
                CancellationToken = cancellationToken,
                AlgorithmParametersConfig = algorithmParams,
                OverrideAlgorithmParametersConfig = createSessionDto.OverrideParametersConfig
            };

            return await PrepareAndRunTestCombinations(executionService, sessionTestData);
        }

        private static async Task CancelIfAnySessionIsRunning(DatabaseOperationExecutionService executionService)
        {
            var runningSessions = await executionService.GetAllSessions((uint)ESessionState.Running);
            if (runningSessions.Any())
            {
                throw new BadRequestException("There is already a running session. Please wait until it finishes or suspend it before starting a new one.");
            }
        }

        private async Task<Session> CreateSessionInDatabase(DatabaseOperationExecutionService executionService)
        {
            var sessionToCreate = new Session()
            {
                StateId = (uint)ESessionState.Running
            };

            await executionService.PerformCreateSessionOperations(sessionToCreate);
            return sessionToCreate;
        }

        private async Task<IEnumerable<SessionTestResultsDto>> PrepareAndRunTestCombinations(DatabaseOperationExecutionService executionService, SessionTestsData invokeSessionTestData)
        {
            ConcurrentBag<SessionTestResultsDto> results = new();

            int availableProcessors = Environment.ProcessorCount;
            int maxParallelTasks = availableProcessors > 3 ? availableProcessors - 2 : 1;
            var sessionTestCombinations = CreateSessionTestCombinations(invokeSessionTestData);

            await CreateSessionTestsInDatabase(executionService, sessionTestCombinations);

            int isFailureHandled = 0;

            await Parallel.ForEachAsync(
                sessionTestCombinations,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxParallelTasks,
                    CancellationToken = invokeSessionTestData.CancellationToken
                },
                async (combo, ct) =>
                {
                    try
                    {
                        results.Add(await InvokeTest(combo, ct));
                    }
                    catch
                    {
                        if (Interlocked.Exchange(ref isFailureHandled, 1) == 0)
                        {
                            await UpdateSessionStateInDatabase(executionService, invokeSessionTestData.SessionId, ESessionState.Suspended);
                        }
                        throw;
                    }
                });

            await UpdateSessionStateInDatabase(executionService, invokeSessionTestData.SessionId, ESessionState.Finished);
            return results.ToList();
        }

        private async Task<SessionTestResultsDto> InvokeTest(SingleSessionTestData testData, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private async Task UpdateSessionStateInDatabase(DatabaseOperationExecutionService executionService, uint sessionId, ESessionState newState)
        {
            await executionService.PerformUpdateSessionStateOperations(sessionId, newState);
        }

        private List<AlgorithmParameterDto> MergeOverrideParameters(
            List<AlgorithmParameterDto> parameters,
            List<AlgorithmParameterDto> overrideParameters)
        {
            var overrideDictionary = overrideParameters.ToDictionary(
                p => p.Id,
                p => p);

            return parameters
                .Select(parameter =>
                    overrideDictionary.TryGetValue(
                        parameter.Id,
                        out var overriddenParameter)
                            ? overriddenParameter
                            : parameter)
                .ToList();
        }

        private List<SingleSessionTestData> CreateSessionTestCombinations(SessionTestsData invokeSessionTestData)
        {
            return invokeSessionTestData.FitnessFunctionsInstances.Keys
                        .SelectMany(
                            functionIds => invokeSessionTestData.AlgorithmInstances.Keys,
                            (functionId, algorithmId) => new SingleSessionTestData
                            {
                                SessionId = invokeSessionTestData.SessionId,
                                AlgorithmId = algorithmId,
                                FitnessFunctionId = functionId,
                                AlgorithmInstance = invokeSessionTestData.AlgorithmInstances[algorithmId],
                                FitnessFunctionInstance = invokeSessionTestData.FitnessFunctionsInstances[functionId],
                                NumberOfRunsPerParameterSet = invokeSessionTestData.NumberOfRunsPerParameterSet,
                                AlgorithmParametersConfig = invokeSessionTestData.OverrideAlgorithmParametersConfig.ContainsKey(algorithmId) ?
                                    MergeOverrideParameters(invokeSessionTestData.AlgorithmParametersConfig[algorithmId], invokeSessionTestData.OverrideAlgorithmParametersConfig[algorithmId]) :
                                invokeSessionTestData.AlgorithmParametersConfig[algorithmId]
                            }).ToList();
        }

        private async Task CreateSessionTestsInDatabase(DatabaseOperationExecutionService executionService, List<SingleSessionTestData> sessionTestCombinations)
        {
            var sessionTestEntities = sessionTestCombinations.Select(c => new SessionTest
            {
                SessionId = c.SessionId,
                AlgorithmId = c.AlgorithmId,
                FitnessFunctionId = c.FitnessFunctionId,
                TestInvokePerParameters = c.NumberOfRunsPerParameterSet,
                ParametersConfig = System.Text.Json.JsonSerializer.Serialize(c.AlgorithmParametersConfig),
                Progress = 0
            }).ToList();

            await executionService.PerformCreateSessionTestsOperations(sessionTestEntities);
        }
    }
}
