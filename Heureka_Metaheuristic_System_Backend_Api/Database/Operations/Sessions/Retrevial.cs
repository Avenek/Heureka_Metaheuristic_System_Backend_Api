using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Sessions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Sessions
{
    public static partial class SessionsDatabaseOperations
    {
        public static async Task<IEnumerable<SessionDto>> GetAllSessions(this DatabaseOperationExecutionService service, uint? stateId)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var mapper = service.GetMappingService<DataDtoMappingService>().Mapper;
            var filterPredicate = stateId == null ? (Expression<Func<Session, bool>>)(s => true) : (s => s.StateId == stateId);
            var sessions = await service.GetEntitiesBy<Session>(filterPredicate, GetSessionDtoSelector()).ToListAsync();

            return mapper.Map<IEnumerable<SessionDto>>(sessions);
        }

        private static Expression<Func<Session, Session>> GetSessionDtoSelector() => s => new Session() { Id = s.Id, StateId = s.StateId, 
            Tests = s.Tests.Select(t => new SessionTest() { Id = t.Id, FitnessFunctionId = t.FitnessFunctionId, AlgorithmId = t.AlgorithmId }).ToList() };
    }
}
