using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.Adapters;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface IAlgorithmService
    {
        Task<IEnumerable<AlgorithmDto>> GetAll();
        Task<AlgorithmDto> GetById(uint id);
        Task<AlgorithmDto> CreateAlgorithm(CreateAlgorithmDto algorithmToCreateDto, IFormFile file);
        Task<AlgorithmDto> UpdateById(uint id, UpdateAlgorithmDto updatedAlgorithmDto);
        Task DeleteById(uint id);
    }

    public class AlgorithmService : IAlgorithmService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly AppSettings appSettings;
        private readonly DllFileLoader dllFileLoader;

        public AlgorithmService(Func<DatabaseOperationExecutionService> executionServiceFactory, AppSettings appSettings, DllFileLoader dllFileLoader)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.appSettings = appSettings;
            this.dllFileLoader = dllFileLoader;
        }

        public async Task<IEnumerable<AlgorithmDto>> GetAll()
        {
            var executionService = executionServiceFactory();
            var algorithmDtos = await executionService.GetAllAlgorithms();
            return algorithmDtos;
        }

        public async Task<AlgorithmDto> GetById(uint id)
        {
            var executionService = executionServiceFactory();
            var algorithmDto = await executionService.GetAlgorithmById(id);
            return algorithmDto;
        }

        public async Task<AlgorithmDto> CreateAlgorithm(CreateAlgorithmDto algorithmToCreateDto, IFormFile file)
        {
            using var executionService = executionServiceFactory();
            await ValidateAlgorithmName(executionService, algorithmToCreateDto.Name);
            DllFileValidator.ValidateFile(file, dllFileLoader.GetAlgorithmFilePath(file.FileName));

            try
            {
                var optimizationType = dllFileLoader.GetOptimizationType<IOptimizationAlgorithm>(file);
                var instance = Activator.CreateInstance(optimizationType)!;
                var algorithmAdapter = new ReflectionOptimizationAlgorithmAdapter(instance);

                var algorithm = new Algorithm()
                {
                    Name = algorithmToCreateDto.Name,
                    FileName = file.FileName,
                    ClassName = optimizationType.Name,
                    IsRemoveable = true,
                    Parameters = algorithmAdapter.ParamsInfo.Select(paramInfo => new AlgorithmParameter()
                    {
                        Name = paramInfo.Name,
                        MinValue = paramInfo.LowerBoundary,
                        MaxValue = paramInfo.UpperBoundary,
                    }).ToList()
                };

                await CreateAlgorithmFile(file);

                if (!await executionService.PerformCreateAlgorithmOperations(algorithm))
                {
                    throw new BadRequestException("Failed to create algorithm.");
                }

                var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
                return mapper.Map<AlgorithmDto>(algorithm);
            }
            catch (Exception)
            {
                DeleteAlgorithmFile(file.FileName);
                throw;
            }
        }

        public async Task DeleteById(uint id)
        {
            var executionService = executionServiceFactory();
            var algorithmDto = await executionService.GetAlgorithmById(id);
            if (!algorithmDto.IsRemoveable)
            {
                throw new BadRequestException("Cannot delete this algorithm.");
            }

            await executionService.PerformDeleteAlgorithmOperations(algorithmDto);
            DeleteAlgorithmFile(algorithmDto.FileName);
        }

        private void DeleteAlgorithmFile(string fileName)
        {
            File.Delete(dllFileLoader.GetAlgorithmFilePath(fileName));
        }

        private async Task CreateAlgorithmFile(IFormFile file)
        {
            var algorithmsFilePath = appSettings.DllPaths.AlgorithmsPath;
            if (!Directory.Exists(algorithmsFilePath))
            {
                Directory.CreateDirectory(algorithmsFilePath);
            }

            string fullPath = dllFileLoader.GetAlgorithmFilePath(file.FileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task<AlgorithmDto> UpdateById(uint id, UpdateAlgorithmDto updatedAlgorithmDto)
        {
            var executionService = executionServiceFactory();
            await ValidateAlgorithmName(executionService, updatedAlgorithmDto.Name);
            await executionService.PerformUpdateAlgorithmOperations(updatedAlgorithmDto, id);
            return await executionService.GetAlgorithmById(id);
        }

        private async Task ValidateAlgorithmName(DatabaseOperationExecutionService executionService, string algorithmName)
        {
            if (string.IsNullOrWhiteSpace(algorithmName))
            {
                throw new BadRequestException("Name cannot be empty.");
            }

            var existsAlgorithmWithSameName = await executionService.GetEntitiesBy<Algorithm>(algorithm => algorithm.Name == algorithmName, a => new Algorithm() { Id = a.Id }).AnyAsync();
            if (existsAlgorithmWithSameName)
            {
                throw new BadRequestException("An algorithm with the same name already exists.");
            }
        }
    }
}
