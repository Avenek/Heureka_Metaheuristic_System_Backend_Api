using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Microsoft.EntityFrameworkCore;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.SessionTests
{
    public static partial class SessionTestDatabaseOperations
    {
        public static async Task<List<SessionTest>> GetSessionTestsDataBySessionId(this DatabaseOperationExecutionService service, uint sessionId)
        {
            var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
            var sessionTests = await service.GetEntitiesBy<SessionTest>(a => a.SessionId == sessionId, s => new SessionTest()
            {
                SessionId = sessionId,
                AlgorithmId = s.AlgorithmId,
                FitnessFunctionId = s.FitnessFunctionId,
                TestInvokePerParameters = s.TestInvokePerParameters,
                ParametersConfig = s.ParametersConfig,
                Algorithm = new Algorithm()
                {
                    Id = s.Algorithm.Id,
                    FileName = s.Algorithm.FileName,
                    ClassName = s.Algorithm.ClassName,
                    Parameters = s.Algorithm.Parameters.Select(p => new AlgorithmParameter() { Id = p.Id, MinValue = p.MinValue, MaxValue = p.MaxValue }).ToList()

                },
                FitnessFunction = new FitnessFunction()
                {
                    Id = s.FitnessFunction.Id,
                    Name = s.FitnessFunction.Name,
                    FileName = s.FitnessFunction.FileName,
                    ClassName = s.FitnessFunction.ClassName,
                    Dimension = s.FitnessFunction.Dimension,
                    DomainPerVariable = s.FitnessFunction.DomainPerVariable
                }
            }).ToListAsync();

            return sessionTests;
        }
    }
}
