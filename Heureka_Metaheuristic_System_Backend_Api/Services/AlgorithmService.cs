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

        public AlgorithmService(Func<DatabaseOperationExecutionService> executionServiceFactory, AppSettings appSettings)
        {
            this.executionServiceFactory = executionServiceFactory;
            this.appSettings = appSettings;
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
            ValidateAlgorithmName(executionService, algorithmToCreateDto.Name);
            ValidateAlgorithmFile(file);
             
            await CreateAlgorithmFile(file);

            try
            {
                var optimizationType = GetOptimizationType(file);
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

                if (!await executionService.PerformCreateAlgorithmOperations(algorithm))
                {
                    throw new BadRequestException("Failed to create algorithm.");
                }

                var mapper = executionService.GetMappingService<DataDtoMappingService>().Mapper;
                return mapper.Map<AlgorithmDto>(algorithm);
            }
            catch (BadRequestException)
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

            DeleteAlgorithmFile(algorithmDto.FileName);
            await executionService.PerformDeleteAlgorithmOperations(algorithmDto);
        }

        private void DeleteAlgorithmFile(string fileName)
        {
            File.Delete(GetAlgorithmFilePath(fileName));
        }

        private Type GetOptimizationType(IFormFile file)
        {
            DllFileLoader fileLoader = new();
            using var readStream = file.OpenReadStream();
            var assembly = fileLoader.LoadFromStream(readStream);
            var types = assembly.GetTypes();
            var optimizationType = types.FirstOrDefault(type => type.GetInterfaces().Any(interfaceType => ReflectionValidator.ImplementsInterface(interfaceType, typeof(IOptimizationAlgorithm))));
            if (optimizationType == null)
            {
                throw new BadRequestException("File does not implement the required interface.");
            }

            return optimizationType;
        }

        private async Task CreateAlgorithmFile(IFormFile file)
        {
            var algorithmsFilePath = appSettings.DllPaths.AlgorithmsPath;
            if (!Directory.Exists(algorithmsFilePath))
            {
                Directory.CreateDirectory(algorithmsFilePath);
            }

            string fullPath = GetAlgorithmFilePath(file.FileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private string GetAlgorithmFilePath(string fileName)
        {
            return Path.Combine(appSettings.DllPaths.AlgorithmsPath, fileName);
        }

        public async Task<AlgorithmDto> UpdateById(uint id, UpdateAlgorithmDto updatedAlgorithmDto)
        {
            var executionService = executionServiceFactory();
            await executionService.PerformUpdateAlgorithmOperations(updatedAlgorithmDto, id);
            return await executionService.GetAlgorithmById(id);
        }

        private void ValidateAlgorithmName(DatabaseOperationExecutionService executionService, string algorithmName)
        {
            var existsAlgorithmWithSameName = executionService.GetEntitiesBy<Algorithm>(algorithm => algorithm.Name == algorithmName, a => new Algorithm() { Id = a.Id }).Any();
            if (existsAlgorithmWithSameName)
            {
                throw new BadRequestException("An algorithm with the same name already exists.");
            }
        }

        private void ValidateAlgorithmFile(IFormFile file)
        {
            if (file == null || file.Name.Length == 0)
            {
                throw new BadRequestException("File is empty.");
            }
            if (Path.GetExtension(file.FileName) != ".dll")
            {
                throw new BadRequestException("File has an invalid extension.");
            }
            if (File.Exists(Path.Combine(appSettings.DllPaths.AlgorithmsPath, file.FileName)))
            {
                throw new BadRequestException("A file with the same name already exists on the server.");
            }
        }
    }
}
