using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.SessionTestResults
{
    public static partial class SessionTestResultDatabaseQueryCollection
    {
        public static async Task<SessionTestResult> GetLastSessionTestResultDataById(this RepositoryCollection repositoryCollection, uint testId)
        {
            var entities = repositoryCollection.Get<SessionTestResult>().Query();

            var sessionTestResult = await entities.Where(e => e.SessionTestId == testId).Select(r => new SessionTestResult()
            {
                Dimension = r.Dimension,
                ParametersGrid = r.ParametersGrid,

            }).LastOrDefaultAsync();

            return sessionTestResult;
        }
    }
}
