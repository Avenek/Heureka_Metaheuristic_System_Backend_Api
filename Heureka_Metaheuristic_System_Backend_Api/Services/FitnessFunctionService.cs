using Heureka_Metaheuristic_System_Backend_Api.Configuration;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;
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

        public async Task<FitnessFunctionDto> CreateFitnessFunction(CreateFitnessFunctionDto fitnessFunctionToCreateDto, IFormFile file)
        {
            using var executionService = executionServiceFactory();
            await ValidateFitnessFunctionName(executionService, fitnessFunctionToCreateDto.Name);
            DllFileValidator.ValidateFile(file, GetFitnessFunctionFilePath(file.FileName));

            try
            {
                var optimizationType = GetOptimizationType(file);
                var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
                var fitnessFunction = mapper.Map<FitnessFunction>(fitnessFunctionToCreateDto);

                fitnessFunction.FileName = file.FileName;
                fitnessFunction.ClassName = optimizationType.Name;
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

        private void DeleteFitnessFunctionFile(string fileName)
        {
            File.Delete(GetFitnessFunctionFilePath(fileName));
        }

        private Type GetOptimizationType(IFormFile file)
        {
            DllFileLoader fileLoader = new();
            using var readStream = file.OpenReadStream();
            var assembly = fileLoader.LoadFromStream(readStream);
            var types = assembly.GetTypes();
            var optimizationType = types.FirstOrDefault(type => type.GetInterfaces().Any(interfaceType => ReflectionValidator.ImplementsInterface(interfaceType, typeof(IFitnessFunction))));
            if (optimizationType == null)
            {
                throw new BadRequestException("File does not implement the required interface.");
            }

            return optimizationType;
        }

        private async Task CreateFitnessFunctionFile(IFormFile file)
        {
            var fitnessFunctionsFilePath = appSettings.DllPaths.FunctionsPath;
            if (!Directory.Exists(fitnessFunctionsFilePath))
            {
                Directory.CreateDirectory(fitnessFunctionsFilePath);
            }

            string fullPath = GetFitnessFunctionFilePath(file.FileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private string GetFitnessFunctionFilePath(string fileName)
        {
            return Path.Combine(appSettings.DllPaths.FunctionsPath, fileName);
        }
        private async Task ValidateFitnessFunctionName(DatabaseOperationExecutionService executionService, string fitnessFunctionName)
        {
            var existsFitnessFunctionWithSameName = await executionService.GetEntitiesBy<FitnessFunction>(fitnessFunction => fitnessFunction.Name == fitnessFunctionName, a => new FitnessFunction() { Id = a.Id }).AnyAsync();
            if (existsFitnessFunctionWithSameName)
            {
                throw new BadRequestException("A fitness function with the same name already exists.");
            }
        }
    }
}
