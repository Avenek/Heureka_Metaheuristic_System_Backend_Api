using Heureka_Metaheuristic_System_Backend_Api.Entities;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.SessionTests
{
    public static partial class SessionTestDatabaseQueryCollection
    {
        public static async Task<bool> UpdateSessionTestProgress(
            this RepositoryCollection resourceRepositoryCollection,
            uint sessionTestId,
            double newProgress
        )
        {
            var context = resourceRepositoryCollection.Context;
            var sessionTestRepository = resourceRepositoryCollection.Get<SessionTest>();
            var sessionTest = await sessionTestRepository.Context.FindAsync<SessionTest>(sessionTestId);

            if (sessionTest is null)
            {
                return false;
            }

            context.Attach(sessionTest);
            sessionTest.Progress = newProgress;

            return await context.SaveChangesAsync() > 0;
        }
    }
}
