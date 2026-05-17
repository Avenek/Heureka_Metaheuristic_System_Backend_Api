using AutoMapper;
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
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;
using System.Text.Json;

public class SessionTestRunner
{
    private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
    private readonly DllFileLoader dllFileLoader;

    private const int PARAMETER_GRID_STEP_COUNT = 5;
    private const int PROGRESS_UPDATE_INTERVAL = 10;
    private const int RESULTS_BATCH_SIZE = 15;

    public SessionTestRunner(Func<DatabaseOperationExecutionService> executionServiceFactory, DllFileLoader dllFileLoader)
    {
        this.executionServiceFactory = executionServiceFactory;
        this.dllFileLoader = dllFileLoader;
    }

    public async Task RunAsync(CreateSessionDto createSessionDto, CancellationToken ct)
    {
        var executionService = executionServiceFactory();

        await EnsureNoRunningSessions(executionService);

        var session = await CreateSession(executionService);

        var sessionTestData = await BuildSessionData(executionService, createSessionDto, ct);

        var testCombinations = CreateSessionTestCombinations(sessionTestData, executionService);

        await CreateSessionTestsInDatabase(executionService, testCombinations);

        await ExecuteAllTests(executionService, session.Id, testCombinations, ct);
    }

    private static async Task EnsureNoRunningSessions(DatabaseOperationExecutionService executionService)
    {
        var running = await executionService.GetAllSessions((uint)ESessionState.Running);

        if (running.Any())
        {
            throw new BadRequestException("There is already a running session.");
        }
    }

    private async Task<Session> CreateSession(DatabaseOperationExecutionService executionService)
    {
        var session = new Session
        {
            StateId = (uint)ESessionState.Running
        };

        await executionService.PerformCreateSessionOperations(session);
        return session;
    }

    private async Task<SessionTestsData> BuildSessionData(DatabaseOperationExecutionService executionService,CreateSessionDto dto, CancellationToken ct)
    {
        var algorithms = await executionService.GetAlgorithmWithParametersByIds(dto.AlgorithmIds);
        var fitnessFunctions = await executionService.GetFitnessFunctionForTestByIds(dto.FitnessFunctionIds);

        if (algorithms.Count != dto.AlgorithmIds.Length)
            throw new NotFoundException("One or more algorithms were not found.");

        if (fitnessFunctions.Count != dto.FitnessFunctionIds.Length)
            throw new NotFoundException("One or more fitness functions were not found.");

        return new SessionTestsData
        {
            Algorithms = algorithms.ToDictionary(a => a.Id, a => a),
            FitnessFunctions = fitnessFunctions.ToDictionary(f => f.Id, f => f),
            NumberOfRunsPerParameterSet = dto.NumberOfRunsPerParameterSet,
            CancellationToken = ct,
            OverrideAlgorithmParametersConfig = dto.OverrideParametersConfig
        };
    }

    private List<SingleSessionTestData> CreateSessionTestCombinations(SessionTestsData sessionData, DatabaseOperationExecutionService executionService)
    {
        var mapper = executionService
            .GetMappingService<DataDtoMappingService>()
            .Mapper;

        var overrides = sessionData.OverrideAlgorithmParametersConfig;

        return sessionData.FitnessFunctions.SelectMany(
            fitness => sessionData.Algorithms,
            (fitnessPair, algorithmPair) =>
            {
                var algorithm = algorithmPair.Value;
                var fitness = fitnessPair.Value;

                var algorithmInstance = new ReflectionOptimizationAlgorithmAdapter(
                    dllFileLoader.CreateInstance(
                        dllFileLoader.GetAlgorithmFilePath(algorithm.FileName),
                        algorithm.ClassName));

                var fitnessInstance = new ReflectionFitnessFunctionAdapter(
                    dllFileLoader.CreateInstance(
                        dllFileLoader.GetFitnessFunctionFilePath(fitness.FileName),
                        fitness.ClassName));

                var parameters = mapper.Map<List<AlgorithmParameterDto>>(algorithm.Parameters);

                if (overrides.TryGetValue(algorithm.Id, out var overrideParams))
                {
                    parameters = MergeOverrideParameters(parameters, overrideParams);
                }

                return new SingleSessionTestData
                {
                    SessionId = sessionData.SessionId,
                    AlgorithmId = algorithm.Id,
                    FitnessFunction = mapper.Map<FitnessFunctionDto>(fitness),
                    AlgorithmInstance = algorithmInstance,
                    FitnessFunctionInstance = fitnessInstance,
                    NumberOfRunsPerParameterSet = sessionData.NumberOfRunsPerParameterSet,
                    AlgorithmParametersConfig = parameters
                };
            }).ToList();
    }

    private List<AlgorithmParameterDto> MergeOverrideParameters(List<AlgorithmParameterDto> parameters, List<AlgorithmParameterDto> overrides)
    {
        var dict = overrides.ToDictionary(x => x.Id, x => x);

        return parameters.Select(p => dict.TryGetValue(p.Id, out var o) ? o : p).ToList();
    }

    private async Task CreateSessionTestsInDatabase(DatabaseOperationExecutionService executionService, List<SingleSessionTestData> tests)
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

    private async Task ExecuteAllTests(DatabaseOperationExecutionService executionService, uint sessionId,List<SingleSessionTestData> tests, CancellationToken ct)
    {
        int failureHandled = 0;

        int maxParallel = Math.Max(1, Environment.ProcessorCount - 2);

        await Parallel.ForEachAsync(
            tests,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallel,
                CancellationToken = ct
            },
            async (test, token) =>
            {
                try
                {
                    await PrepareAndRunTests(executionService, test, token);
                }
                catch
                {
                    if (Interlocked.Exchange(ref failureHandled, 1) == 0)
                    {
                        await UpdateSessionState(executionService, sessionId, ESessionState.Suspended);
                    }
                    throw;
                }
            });

        await UpdateSessionState(executionService, sessionId, ESessionState.Finished);
    }

    private async Task PrepareAndRunTests(DatabaseOperationExecutionService executionService,SingleSessionTestData test, CancellationToken ct)
    {
        int minDim = GetMinimalDimension(test);

        double[] parameters = test.AlgorithmParametersConfig
            .Select(p => p.MinValue)
            .ToArray();

        await InvokeTests(executionService, test, minDim, parameters, ct);
    }

    private async Task InvokeTests(DatabaseOperationExecutionService executionService, SingleSessionTestData test, int startDim, double[] parameters, CancellationToken ct)
    {
        var resultsBuffer = new List<SessionTestResult>();

        int minDim = GetMinimalDimension(test);
        int maxDim = test.FitnessFunction.Dimension ?? 27;

        int step = test.FitnessFunction.Dimension.HasValue ? 1 : (maxDim - minDim) / PARAMETER_GRID_STEP_COUNT;

        int iterations = 0;

        double totallIterations = Math.Pow(PARAMETER_GRID_STEP_COUNT, test.AlgorithmParametersConfig.Count);

        try
        {
            for (int dim = startDim; dim <= maxDim; dim += step)
            {
                ct.ThrowIfCancellationRequested();

                var result = await InvokeTestForParameters(test, parameters, (uint)dim, ct);
                resultsBuffer.Add(result);

                if (resultsBuffer.Count >= RESULTS_BATCH_SIZE)
                {
                    await executionService.PerformCreateSessionTestResultsOperations(resultsBuffer);
                    resultsBuffer.Clear();
                }

                if (TryIncreaseParams(parameters, test.AlgorithmParametersConfig, out parameters))
                {
                    iterations++;

                    if (iterations % PROGRESS_UPDATE_INTERVAL == 0)
                    {
                        var progress = iterations / totallIterations;
                        await executionService.PerformUpdateSessionTestProgressOperations(test.TestId, progress);
                    }
                }
                else
                {
                    await executionService.PerformUpdateSessionTestProgressOperations(test.TestId, 1);
                    break;
                }
            }
        }
        catch
        {
            if (resultsBuffer.Count != 0)
            {
                await executionService.PerformCreateSessionTestResultsOperations(resultsBuffer);
            }
            throw;
        }
    }

    private async Task<SessionTestResult> InvokeTestForParameters(SingleSessionTestData test, double[] parameters, uint dimension, CancellationToken ct)
    {
        var iterations = new List<TestIterationResults>();

        var domain = GetFunctionDomain(test.FitnessFunction.DomainPerVariable, dimension);

        for (int i = 0; i < test.NumberOfRunsPerParameterSet; i++)
        {
            ct.ThrowIfCancellationRequested();

            test.AlgorithmInstance.Solve(
                test.FitnessFunctionInstance,
                dimension,
                domain,
                parameters);

            iterations.Add(new TestIterationResults
            {
                XBest = test.AlgorithmInstance.XBest,
                FBest = test.AlgorithmInstance.FBest,
                FitnessFunctionEvaluations = test.AlgorithmInstance.NumberOfEvaluationFitnessFunction,
                Dimension = dimension,
                ParameterValues = parameters.ToArray()
            });
        }

        var best = iterations.OrderBy(x => x.FBest).First();

        var grid = new Dictionary<uint, double>();

        for (int i = 0; i < test.AlgorithmParametersConfig.Count; i++)
        {
            grid.Add(test.AlgorithmParametersConfig[i].Id, best.ParameterValues[i]);
        }

        return new SessionTestResult
        {
            SessionTestId = test.TestId,
            XBest = string.Join(';', best.XBest),
            FBest = best.FBest,
            Dimension = dimension,
            FitnessFunctionEvaluations = best.FitnessFunctionEvaluations,
            ParametersGrid = JsonSerializer.Serialize(grid)
        };
    }

    private async Task UpdateSessionState(DatabaseOperationExecutionService executionService, uint sessionId, ESessionState state)
    {
        await executionService.PerformUpdateSessionStateOperations(sessionId, state);
    }

    private bool TryIncreaseParams(double[] parameters, List<AlgorithmParameterDto> config, out double[] result)
    {
        result = parameters;

        for (int i = parameters.Length - 1; i >= 0; i--)
        {
            var step = (config[i].MaxValue - config[i].MinValue) / PARAMETER_GRID_STEP_COUNT;

            parameters[i] += step;

            if (parameters[i] >= config[i].MaxValue)
            {
                parameters[i] = config[i].MinValue;
            }
            else
            {
                result = parameters;
                return true;
            }
        }

        return false;
    }

    private double[,] GetFunctionDomain(List<FitnessFunctionDomainDto> domain, uint dim)
    {
        const double MIN = -1_000_000;
        const double MAX = 1_000_000;

        var result = new double[dim, 2];

        if (domain == null || domain.Count == 0)
        {
            for (int i = 0; i < dim; i++)
            {
                result[i, 0] = MIN;
                result[i, 1] = MAX;
            }
            return result;
        }

        for (int i = 0; i < dim; i++)
        {
            var d = i < domain.Count ? domain[i] : domain[0];

            result[i, 0] = d.Minimum ?? MIN;
            result[i, 1] = d.Maximum ?? MAX;
        }

        return result;
    }

    private int GetMinimalDimension(SingleSessionTestData test) => test.FitnessFunction.Dimension ?? 2;
}