using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.SessionTestResults;
using Microsoft.EntityFrameworkCore;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTestResults
{
    public static partial class SessionTestResultDatabaseOperations
    {
        public static async Task<SessionTestResult> GetLastSessionTestResultDataById(this DatabaseOperationExecutionService service, uint testId)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var sessionTestResult = await repositoryCollection.GetLastSessionTestResultDataById(testId);
            return sessionTestResult;
        }
    }
}
