using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface IFitnessFunctionService
    {
        Task<IEnumerable<FitnessFunctionDto>> GetAll();
        Task<FitnessFunctionDto> GetById(uint id);
    }

    public class FitnessFunctionService : IFitnessFunctionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly AppSettings appSettings;

        public FitnessFunctionService(Func<DatabaseOperationExecutionService> executionServiceFactory, AppSettings appSettings)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.appSettings = appSettings;
        }

        public async Task<IEnumerable<FitnessFunctionDto>> GetAll()
        {
            var executionService = executionServiceFactory();
            var fitnessFunctionDtos = await executionService.GetAllFitnessFunctions();
            return fitnessFunctionDtos;
        }
        public async Task<FitnessFunctionDto> GetById(uint id)
        {
            var executionService = executionServiceFactory();
            var fitnessFunctionDto = await executionService.GetFitnessFunctionById(id);
            return fitnessFunctionDto;
        }
    }
}
