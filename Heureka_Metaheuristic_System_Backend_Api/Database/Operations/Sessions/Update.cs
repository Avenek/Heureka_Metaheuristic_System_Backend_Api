using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Enums;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions
{
    public static partial class SessionsDatabaseOperations
    {
        private async static Task<bool> UpdateSession(
            this DatabaseOperationExecutionService service,
            uint sessionId, uint newSessionState
        )
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var updatedSession = await repositoryCollection.Get<Session>().GetById(sessionId);
            updatedSession.StateId = newSessionState;
            var result = await repositoryCollection.UpdateEntity(updatedSession);
            return result;
        }

        public async static Task PerformUpdateSessionStateOperations(
           this DatabaseOperationExecutionService service,
            uint sessionId, ESessionState newSessionState
        )
        {
            Func<Task<bool>>[] operations =
            [
                async () => await service.UpdateSession(sessionId, (uint)newSessionState),
            ];

            await service.CommitAllOrRollbackAsync(operations: operations);
        }
    }
}
