using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTestResults;
using System.Text.Json;

namespace Heureka_Metaheuristic_System_Backend_Api.Helpers
{
    public class ResumeTestRunner : SingleSessionTestRunner
    {
        private readonly DatabaseOperationExecutionService executionService;

        public ResumeTestRunner(DatabaseOperationExecutionService executionService)
        {
            this.executionService = executionService;
            PrepareInitializeParameters();
        }

        public override async Task PrepareInitializeParameters()
        {
            var lastSessionTestResult = await executionService.GetLastSessionTestResultDataById(TestId);
            var parametersPerId = JsonSerializer.Deserialize<Dictionary<uint, double>>(lastSessionTestResult.ParametersGrid);
            InitialParameters = parametersPerId.OrderBy(p => p.Key).Select(p => p.Value).ToArray();
            InitialDimension = (int)lastSessionTestResult.Dimension;
        }
    }
}
