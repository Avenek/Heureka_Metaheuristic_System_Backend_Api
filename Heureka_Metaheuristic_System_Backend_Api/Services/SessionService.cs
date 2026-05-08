using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionDto>> GetAll(uint? stateId);
        Task DeleteById(uint id);
    }

    public class SessionService : ISessionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;

        public SessionService(Func<DatabaseOperationExecutionService> executionServiceFactory)
        {
            this.executionServiceFactory = executionServiceFactory;
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
    }
}
