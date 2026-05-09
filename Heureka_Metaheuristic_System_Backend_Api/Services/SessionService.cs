using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.DataClasses;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Enums;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using NLog.LayoutRenderers.Wrappers;

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
        private readonly AppSettings appSettings;
        private readonly DllFileLoader dllFileLoader;

        public SessionService(Func<DatabaseOperationExecutionService> executionServiceFactory, AppSettings appSettings, DllFileLoader dllFileLoader)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.appSettings = appSettings;
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
            if (createSessionDto.AlgorithmIds.Length == 1)
            {
                return await PerformAlgorithmTest(createSessionDto, cancellationToken);
            }
            else if (createSessionDto.FitnessFunctionIds.Length == 1)
            {
                return await PerformFitnessFunctionTest(createSessionDto, cancellationToken);
            }
            else
            {
                throw new BadRequestException("You must provide exactly one algorithm id or one fitness function id.");
            }
        }

        private async Task<IEnumerable<SessionTestResultsDto>> PerformAlgorithmTest(CreateSessionDto createSessionDto, CancellationToken cancellationToken)
        {
            var testedAlgorithmId = createSessionDto.AlgorithmIds[0];
            var executionService = executionServiceFactory();
            var algorithm = await executionService.GetAlgorithmWithParametersById(testedAlgorithmId);
            var fitnessFunctions = await executionService.GetFitnessFunctionByIds(createSessionDto.FitnessFunctionIds);

            if (fitnessFunctions.Count != createSessionDto.FitnessFunctionIds.Length)
            {
                throw new NotFoundException("One or more fitness functions with the provided ids were not found.");
            }

            var algorithmType = dllFileLoader.GetOptimizationType<IOptimizationAlgorithm>(dllFileLoader.GetAlgorithmFilePath(algorithm.FileName));
            var functionTypes = fitnessFunctions
                .Select(ff => new KeyValuePair<uint, Type>(ff.Id, dllFileLoader.GetOptimizationType<IFitnessFunction>(dllFileLoader.GetFitnessFunctionFilePath(ff.FileName))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            await CancelIfAnySessionIsRunning(executionService);
            var createdSession = await CreateSessionInDatabase(executionService);
            var invokeSessionTestData = new SessionTestData()
            {
                SessionId = createdSession.Id,
                AlgorithmTypes = new() { { testedAlgorithmId, algorithmType } },
                FitnessFunctionsTypes = functionTypes,
                NumberOfRunsPerParameterSet = createSessionDto.NumberOfRunsPerParameterSet,
                CancellationToken = cancellationToken
            };
            var results = await PrepareDataAndInvokeTests(executionService, invokeSessionTestData);
            return results;
        }

        private async Task<IEnumerable<SessionTestResultsDto>> PerformFitnessFunctionTest(CreateSessionDto createSessionDto, CancellationToken cancellationToken)
        {
            var testedFitnessFunctionId = createSessionDto.FitnessFunctionIds[0];
            var executionService = executionServiceFactory();
            var fitnessFunction = await executionService.GetFitnessFunctionById(testedFitnessFunctionId);
            var algorithms = await executionService.GetAlgorithmWithParametersByIds(createSessionDto.AlgorithmIds);

            if (algorithms.Count != createSessionDto.AlgorithmIds.Length)
            {
                throw new NotFoundException("One or more algorithms with the provided ids were not found.");
            }

            var fitnessFunctionType = dllFileLoader.GetOptimizationType<IFitnessFunction>(dllFileLoader.GetFitnessFunctionFilePath(fitnessFunction.FileName));
            var algorithmTypes = algorithms
                .Select(a => new KeyValuePair<uint, Type>(a.Id, dllFileLoader.GetOptimizationType<IOptimizationAlgorithm>(dllFileLoader.GetAlgorithmFilePath(a.FileName))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            await CancelIfAnySessionIsRunning(executionService);
            var createdSession = await CreateSessionInDatabase(executionService);
            var invokeSessionTestData = new SessionTestData()
            {
                SessionId = createdSession.Id,
                AlgorithmTypes = algorithmTypes,
                FitnessFunctionsTypes = new() { { testedFitnessFunctionId, fitnessFunctionType } },
                NumberOfRunsPerParameterSet = createSessionDto.NumberOfRunsPerParameterSet,
                CancellationToken = cancellationToken
            };
            var results = await PrepareDataAndInvokeTests(executionService, invokeSessionTestData);
            return results;
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

        private async Task<IEnumerable<SessionTestResultsDto>> PrepareDataAndInvokeTests(DatabaseOperationExecutionService executionService, SessionTestData invokeSessionTestData)
        {
            var sessionTestResultsDtos = new List<SessionTestResultsDto>();
            //TODO: Implement
            throw new NotImplementedException();

            await UpdateSessionStateInDatabase(executionService, invokeSessionTestData.SessionId, ESessionState.Finished);
            return sessionTestResultsDtos;
        }

        private async Task UpdateSessionStateInDatabase(DatabaseOperationExecutionService executionService, uint sessionId, ESessionState newState)
        {
            await executionService.PerformUpdateSessionStateOperations(sessionId, newState);
        }
    }
}
