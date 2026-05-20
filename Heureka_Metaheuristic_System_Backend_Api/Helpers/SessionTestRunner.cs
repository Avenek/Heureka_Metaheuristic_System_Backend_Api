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

    public async Task ResumeSessionAsync(uint sessionId, CancellationToken ct)
    {
        var executionService = executionServiceFactory();
        await EnsureNoRunningSessions(executionService);

        var sessionData = await executionService.GetSessionTestsDataBySessionId(sessionId);

        var sessionTestData = new SessionTestsData
        {
            SessionId = sessionId,
            Algorithms = sessionData.ToDictionary(s => s.AlgorithmId, s => s.Algorithm),
            FitnessFunctions = sessionData.ToDictionary(s => s.FitnessFunctionId, s => s.FitnessFunction),
            NumberOfRunsPerParameterSet = sessionData.First().TestInvokePerParameters,
            CancellationToken = ct,
            OverrideAlgorithmParametersConfig = sessionData.ToDictionary(
                s => s.AlgorithmId,
                s => JsonSerializer.Deserialize<List<AlgorithmParameterDto>>(s.ParametersConfig) ?? new List<AlgorithmParameterDto>())
        };

        var testCombinations = CreateSessionTestCombinations(sessionTestData, executionService);

        await ExecuteAllTests(executionService, sessionId, testCombinations, ct);
    }

    private static async Task EnsureNoRunningSessions(DatabaseOperationExecutionService executionService)
    {
        var running = await executionService.GetAllSessions((uint)ESessionState.Running);

        if (running.Any())
        {
            throw new BadRequestException("There is already a running session.");
        }
    }

    public List<SingleSessionTestRunner> CreateSessionTestCombinations(SessionTestsData sessionData, DatabaseOperationExecutionService executionService)
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

                return new SingleSessionTestRunner()
                {
                    SessionId = sessionData.SessionId,
                    AlgorithmId = algorithm.Id,
                    FitnessFunction = mapper.Map<FitnessFunctionDto>(fitness),
                    AlgorithmInstance = algorithmInstance,
                    FitnessFunctionInstance = fitnessInstance,
                    NumberOfRunsPerParameterSet = sessionData.NumberOfRunsPerParameterSet,
                    AlgorithmParametersConfig = parameters,
                };
            }).ToList();
    }

    private List<AlgorithmParameterDto> MergeOverrideParameters(List<AlgorithmParameterDto> parameters, List<AlgorithmParameterDto> overrides)
    {
        var dict = overrides.ToDictionary(x => x.Id, x => x);

        return parameters.Select(p => dict.TryGetValue(p.Id, out var o) ? o : p).ToList();
    }

    private async Task ExecuteAllTests(DatabaseOperationExecutionService executionService, uint sessionId, List<SingleSessionTestRunner> tests, CancellationToken ct)
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
                    await PrepareInitializeParameters(executionService,  test);
                    await InvokeTests(executionService, test, ct);
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

    public async Task PrepareInitializeParameters(DatabaseOperationExecutionService executionService, SingleSessionTestRunner test)
    {
        var lastSessionTestResult = await executionService.GetLastSessionTestResultDataById(test.TestId);
        if (lastSessionTestResult != null)
        {
            var parametersPerId = JsonSerializer.Deserialize<Dictionary<uint, double>>(lastSessionTestResult.ParametersGrid);
            test.InitialParameters = parametersPerId.OrderBy(p => p.Key).Select(p => p.Value).ToArray();
            test.InitialDimension = (int)lastSessionTestResult.Dimension;
        }
        else
        {
            test.InitialParameters = test.AlgorithmParametersConfig.Select(p => p.MinValue).ToArray();
            test.InitialDimension = test.MinimalDimension;
        }
    }

    private async Task InvokeTests(DatabaseOperationExecutionService executionService, SingleSessionTestRunner test, CancellationToken ct)
    {
        var resultsBuffer = new List<SessionTestResult>();
        int maxDim = test.FitnessFunction.Dimension ?? 27;
        int dimensionStep = test.FitnessFunction.Dimension.HasValue ? 1 : (maxDim - test.MinimalDimension) / PARAMETER_GRID_STEP_COUNT;
        int iterations = 0;
        double totallIterations = Math.Pow(PARAMETER_GRID_STEP_COUNT, test.AlgorithmParametersConfig.Count);
        var parameters = test.InitialParameters.ToArray();

        try
        {
            for (int dim = test.InitialDimension; dim <= maxDim; dim += dimensionStep)
            {
                ct.ThrowIfCancellationRequested();

                var result = await InvokeTestForParameters(test, parameters,(uint)dim, ct);
                resultsBuffer.Add(result);

                if (resultsBuffer.Count >= RESULTS_BATCH_SIZE)
                {
                    await CreateResultsTestBatch(executionService, resultsBuffer);
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
            throw;
        }
        finally
        {
            if (resultsBuffer.Count != 0)
            {
                await CreateResultsTestBatch(executionService, resultsBuffer);
            }
        }
    }

    private async Task CreateResultsTestBatch(DatabaseOperationExecutionService executionService, List<SessionTestResult> resultsBuffer)
    {
        await executionService.PerformCreateSessionTestResultsOperations(resultsBuffer);
        resultsBuffer.Clear();
    }

    private async Task<SessionTestResult> InvokeTestForParameters(SingleSessionTestRunner test, double[] parameters, uint dimension, CancellationToken ct)
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
            ParametersGrid = JsonSerializer.Serialize(grid.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value))
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
}