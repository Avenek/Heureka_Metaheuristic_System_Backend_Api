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
        Task CreateSession(CreateSessionDto createSessionDto, CancellationToken cancellationToken);
    }

    public class SessionService : ISessionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly DllFileLoader dllFileLoader;
        private const int PARAMETER_GRID_STEP_COUNT = 5;
        private const int PROGRESS_UPDATE_INTERVAL = 10;
        private const int RESULTS_BATCH_SIZE = 15;

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

        public async Task CreateSession(CreateSessionDto createSessionDto, CancellationToken cancellationToken)
        {
            var executionService = executionServiceFactory();

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

            var sessionTestData = new SessionTestsData
            {
                SessionId = createdSession.Id,
                Algorithms = algorithms.ToDictionary(a => a.Id, a => a),
                FitnessFunctions = fitnessFunctions.ToDictionary(f => f.Id, f => f),
                NumberOfRunsPerParameterSet = createSessionDto.NumberOfRunsPerParameterSet,
                CancellationToken = cancellationToken,
                OverrideAlgorithmParametersConfig = createSessionDto.OverrideParametersConfig
            };

            await PrepareAndRunTestCombinations(executionService, sessionTestData);
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

        private async Task PrepareAndRunTestCombinations(DatabaseOperationExecutionService executionService, SessionTestsData invokeSessionTestData)
        {
            int availableProcessors = Environment.ProcessorCount;
            int maxParallelTasks = availableProcessors > 3 ? availableProcessors - 2 : 1;
            var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
            var sessionTestCombinations = CreateSessionTestCombinations(invokeSessionTestData, mapper);

            await CreateSessionTestsInDatabase(executionService, sessionTestCombinations);

            int isFailureHandled = 0;

            await Parallel.ForEachAsync(
                sessionTestCombinations,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxParallelTasks,
                    CancellationToken = invokeSessionTestData.CancellationToken
                },
                async (testCombination, cancellationToken) =>
                {
                    try
                    {
                        await PrepareParametersAndInvokeTests(executionService, testCombination, cancellationToken);
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
        }

        private async Task PrepareParametersAndInvokeTests(DatabaseOperationExecutionService executionService, SingleSessionTestData testData, CancellationToken cancellationToken)
        {
            List<SessionTestResult> pendingResults = new();

            var isDimensionSet = testData.FitnessFunction.Dimension.HasValue;
            var minDimension = isDimensionSet ? testData.FitnessFunction.Dimension!.Value : 2;
            var maxDimension = isDimensionSet ? testData.FitnessFunction.Dimension!.Value : 27;
            var dimensionStepCount = isDimensionSet ? 1 : (maxDimension - minDimension) / PARAMETER_GRID_STEP_COUNT;

            var testingParametersCount = testData.AlgorithmParametersConfig.Count;
            if (!isDimensionSet)
            {
                testingParametersCount++;
            }

            var allIterationsCount = Math.Pow(PARAMETER_GRID_STEP_COUNT, testingParametersCount);

            double[] parametersForTest = testData.AlgorithmParametersConfig.Select(p => p.MinValue).ToArray();
            var iteration = 0;
            try
            {
                for (int dimension = minDimension; dimension <= maxDimension; dimension += dimensionStepCount)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var testResult = await InvokeTestForParameters(testData, parametersForTest, (uint)dimension, cancellationToken);
                    pendingResults.Add(testResult);
                    if (pendingResults.Count >= RESULTS_BATCH_SIZE)
                    {
                        await executionService.PerformCreateSessionTestResultsOperations(pendingResults);
                        pendingResults.Clear();
                    }

                    if (TryIncreaseAlgorithmParamsForTest(parametersForTest, testData.AlgorithmParametersConfig, out parametersForTest))
                    {
                        iteration++;
                        if (iteration % PROGRESS_UPDATE_INTERVAL == 0)
                        {
                            double progress = iteration / allIterationsCount;
                            await executionService.PerformUpdateSessionTestProgressOperations(testData.TestId, progress);
                        }
                    }
                    else
                    {
                        await executionService.PerformUpdateSessionTestProgressOperations(testData.TestId, 1);
                        break;
                    }
                }
            }
            catch
            {
                if (pendingResults.Count > 0)
                {
                    await executionService.PerformCreateSessionTestResultsOperations(pendingResults);
                }

                throw;
            }

        }

        private async Task<SessionTestResult> InvokeTestForParameters(SingleSessionTestData testData, double[] testParameters, uint dimension, CancellationToken cancellationToken)
        {
            List<TestIterationResults> iterationResults = new();

            double[,] domain = GetFunctionDomain(testData.FitnessFunction.DomainPerVariable, dimension);
            for (int iter = 0; iter < testData.NumberOfRunsPerParameterSet; iter++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                testData.AlgorithmInstance.Solve(testData.FitnessFunctionInstance, dimension, domain, testParameters);
                TestIterationResults iterParams = new()
                {
                    XBest = testData.AlgorithmInstance.XBest,
                    FBest = testData.AlgorithmInstance.FBest,
                    FitnessFunctionEvaluations = testData.AlgorithmInstance.NumberOfEvaluationFitnessFunction,
                    Dimension = dimension,
                    ParameterValues = testParameters.ToArray()
                };
                iterationResults.Add(iterParams);
            }

            var bestIterationResult = iterationResults.OrderBy(param => param.FBest).First();
            var parametersGrid = new Dictionary<uint, double>();
            for (int i = 0; i < testData.AlgorithmParametersConfig.Count; i++)
            {
                parametersGrid.Add(testData.AlgorithmParametersConfig[i].Id, bestIterationResult.ParameterValues[i]);
            }
            ;
            SessionTestResult testResult = new()
            {
                SessionTestId = testData.TestId,
                XBest = String.Join(';', bestIterationResult.XBest),
                FBest = bestIterationResult.FBest,
                Dimension = (uint)bestIterationResult.Dimension,
                FitnessFunctionEvaluations = bestIterationResult.FitnessFunctionEvaluations,
                ParametersGrid = JsonSerializer.Serialize(parametersGrid)
            };
            return testResult;
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

        private List<SingleSessionTestData> CreateSessionTestCombinations(SessionTestsData sessionData, IMapper mapper)
        {
            var algorithms = sessionData.Algorithms;
            var fitnessFunctions = sessionData.FitnessFunctions;
            var overrides = sessionData.OverrideAlgorithmParametersConfig;

            return fitnessFunctions.SelectMany(
                functionPair => algorithms,
                (functionPair, algorithmPair) =>
                {
                    var functionId = functionPair.Key;
                    var fitnessFunction = functionPair.Value;
                    var fitnessFunctionDto = mapper.Map<FitnessFunctionDto>(fitnessFunction);

                    var algorithmId = algorithmPair.Key;
                    var algorithm = algorithmPair.Value;

                    var algorithmInstance = new ReflectionOptimizationAlgorithmAdapter(
                        dllFileLoader.CreateInstance(
                            dllFileLoader.GetAlgorithmFilePath(algorithm.FileName),
                            algorithm.ClassName));

                    var fitnessFunctionInstance = new ReflectionFitnessFunctionAdapter(
                        dllFileLoader.CreateInstance(
                            dllFileLoader.GetFitnessFunctionFilePath(fitnessFunction.FileName),
                            fitnessFunction.ClassName));

                    var algorithmParameters =
                        mapper.Map<List<AlgorithmParameterDto>>(algorithm.Parameters);

                    if (overrides.TryGetValue(algorithmId, out var overrideConfig))
                    {
                        algorithmParameters = MergeOverrideParameters(
                            algorithmParameters,
                            overrideConfig);
                    }

                    return new SingleSessionTestData
                    {
                        SessionId = sessionData.SessionId,
                        AlgorithmId = algorithmId,
                        FitnessFunction = fitnessFunctionDto,
                        AlgorithmInstance = algorithmInstance,
                        FitnessFunctionInstance = fitnessFunctionInstance,
                        NumberOfRunsPerParameterSet = sessionData.NumberOfRunsPerParameterSet,
                        AlgorithmParametersConfig = algorithmParameters
                    };
                })
                .ToList();
        }

        private async Task CreateSessionTestsInDatabase(DatabaseOperationExecutionService executionService, List<SingleSessionTestData> sessionTestCombinations)
        {
            var sessionTestEntities = sessionTestCombinations.Select(c => new SessionTest
            {
                SessionId = c.SessionId,
                AlgorithmId = c.AlgorithmId,
                FitnessFunctionId = c.FitnessFunction.Id,
                TestInvokePerParameters = c.NumberOfRunsPerParameterSet,
                ParametersConfig = System.Text.Json.JsonSerializer.Serialize(c.AlgorithmParametersConfig),
                Progress = 0
            }).ToList();

            await executionService.PerformCreateSessionTestsOperations(sessionTestEntities);

            foreach (var sessionTestCombination in sessionTestCombinations)
            {
                var createdTest = sessionTestEntities.First(t => t.SessionId == sessionTestCombination.SessionId
                                                                && t.AlgorithmId == sessionTestCombination.AlgorithmId
                                                                && t.FitnessFunctionId == sessionTestCombination.FitnessFunction.Id);
                sessionTestCombination.TestId = createdTest.Id;

            }
        }

        private bool TryIncreaseAlgorithmParamsForTest(double[] parametersForTest, List<AlgorithmParameterDto> algorithmParametersConfig, out double[] result)
        {
            result = parametersForTest;

            for (int i = parametersForTest.Length - 1; i >= 0; i--)
            {
                var parameterConfig = algorithmParametersConfig[i];
                parametersForTest[i] += (parameterConfig.MaxValue - parameterConfig.MinValue) / PARAMETER_GRID_STEP_COUNT;
                if (parametersForTest[i] >= parameterConfig.MaxValue)
                {
                    parametersForTest[i] = parameterConfig.MinValue;
                }
                else
                {
                    result = parametersForTest;
                    return true;
                }
            }

            return false;
        }

        private double[,] GetFunctionDomain(List<FitnessFunctionDomainDto> domainPerVariable, uint dimension)
        {
            const double DEFAULT_MIN = -1_000_000;
            const double DEFAULT_MAX = 1_000_000;

            var domain = new double[dimension, 2];

            if (domainPerVariable == null || domainPerVariable.Count == 0)
            {
                for (int i = 0; i < dimension; i++)
                {
                    domain[i, 0] = DEFAULT_MIN;
                    domain[i, 1] = DEFAULT_MAX;
                }

                return domain;
            }

            if (domainPerVariable.Count == 1)
            {
                var singleDomain = domainPerVariable[0];

                double min = singleDomain.Minimum ?? DEFAULT_MIN;
                double max = singleDomain.Maximum ?? DEFAULT_MAX;

                for (int i = 0; i < dimension; i++)
                {
                    domain[i, 0] = min;
                    domain[i, 1] = max;
                }

                return domain;
            }

            for (int i = 0; i < dimension; i++)
            {
                if (i < domainPerVariable.Count)
                {
                    var variableDomain = domainPerVariable[i];

                    domain[i, 0] = variableDomain.Minimum ?? DEFAULT_MIN;
                    domain[i, 1] = variableDomain.Maximum ?? DEFAULT_MAX;
                }
                else
                {
                    domain[i, 0] = DEFAULT_MIN;
                    domain[i, 1] = DEFAULT_MAX;
                }
            }

            return domain;
        }
    }
}
