using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Reflection;
using Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Heureka_Metaheuristic_System_Backend_Api.Services
{
    public interface IFitnessFunctionService
    {
        Task<IEnumerable<FitnessFunctionDto>> GetAll();
        Task<FitnessFunctionDto> GetById(uint id);
        Task<FitnessFunctionDto> CreateFitnessFunction(CreateFitnessFunctionDto fitnessFunctionToCreateDto, IFormFile file);
        Task<FitnessFunctionDto> UpdateById(uint id, UpdateFitnessFunctionDto updatedFitnessFunctionDto);
        Task DeleteById(uint id);
    }

    public class FitnessFunctionService : IFitnessFunctionService
    {
        private readonly Func<DatabaseOperationExecutionService> executionServiceFactory;
        private readonly AppSettings appSettings;
        private readonly DllFileLoader dllFileLoader;

        public FitnessFunctionService(Func<DatabaseOperationExecutionService> executionServiceFactory, AppSettings appSettings, DllFileLoader dllFileLoader)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.appSettings = appSettings;
            this.dllFileLoader = dllFileLoader;
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

        public async Task<FitnessFunctionDto> CreateFitnessFunction(CreateFitnessFunctionDto fitnessFunctionToCreateDto, IFormFile file)
        {
            using var executionService = executionServiceFactory();
            await ValidateFitnessFunctionName(executionService, fitnessFunctionToCreateDto.Name);
            DllFileValidator.ValidateFile(file, dllFileLoader.GetFitnessFunctionFilePath(file.FileName));

            try
            {
                var optimizationType = dllFileLoader.GetOptimizationType<IFitnessFunction>(file);
                var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
                var fitnessFunction = mapper.Map<FitnessFunction>(fitnessFunctionToCreateDto);

                fitnessFunction.FileName = file.FileName;
                fitnessFunction.ClassName = optimizationType.FullName;
                fitnessFunction.IsRemoveable = true;

                await CreateFitnessFunctionFile(file);

                if (!await executionService.PerformCreateFitnessFunctionOperations(fitnessFunction))
                {
                    throw new BadRequestException("Failed to create fitnessFunction.");
                }

                return mapper.Map<FitnessFunctionDto>(fitnessFunction);
            }
            catch (Exception)
            {
                DeleteFitnessFunctionFile(file.FileName);
                throw;
            }
        }

        public async Task<FitnessFunctionDto> UpdateById(uint id, UpdateFitnessFunctionDto updatedFitnessFunctionDto)
        {
            var executionService = executionServiceFactory();
            await ValidateFitnessFunctionName(executionService, updatedFitnessFunctionDto.Name);
            await executionService.PerformUpdateFitnessFunctionOperations(updatedFitnessFunctionDto, id);
            return await executionService.GetFitnessFunctionById(id);
        }

        public async Task DeleteById(uint id)
        {
            var executionService = executionServiceFactory();
            var fitnessFunctionDto = await executionService.GetFitnessFunctionById(id);
            if (!fitnessFunctionDto.IsRemoveable)
            {
                throw new BadRequestException("Cannot delete this fitness function.");
            }

            await executionService.PerformDeleteFitnessFunctionOperations(fitnessFunctionDto);
            DeleteFitnessFunctionFile(fitnessFunctionDto.FileName);
        }


        private void DeleteFitnessFunctionFile(string fileName)
        {
            File.Delete(dllFileLoader.GetFitnessFunctionFilePath(fileName));
        }

        private async Task CreateFitnessFunctionFile(IFormFile file)
        {
            var fitnessFunctionsFilePath = appSettings.DllPaths.FunctionsPath;
            if (!Directory.Exists(fitnessFunctionsFilePath))
            {
                Directory.CreateDirectory(fitnessFunctionsFilePath);
            }

            string fullPath = dllFileLoader.GetFitnessFunctionFilePath(file.FileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private async Task ValidateFitnessFunctionName(DatabaseOperationExecutionService executionService, string fitnessFunctionName)
        {
            if (string.IsNullOrWhiteSpace(fitnessFunctionName))
            {
                throw new BadRequestException("Name cannot be empty.");
            }

            var existsFitnessFunctionWithSameName = await executionService.GetEntitiesBy<FitnessFunction>(fitnessFunction => fitnessFunction.Name == fitnessFunctionName, a => new FitnessFunction() { Id = a.Id }).AnyAsync();
            if (existsFitnessFunctionWithSameName)
            {
                throw new BadRequestException("A fitness function with the same name already exists.");
            }
        }
    }
}
