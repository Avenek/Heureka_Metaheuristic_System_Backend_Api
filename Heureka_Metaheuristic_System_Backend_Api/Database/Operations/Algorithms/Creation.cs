

using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.Database.QueryCollections.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Entities;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Heureka_Metaheuristic_System_Backend_Api.MappingProfiles;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;

namespace Heureka_Metaheuristic_System_Backend_Api.Database.Operations.Algorithms
{
    public static partial class AlgorithmDatabaseOperations
    {
		private async static Task<bool> CreateAlgorithm(
			this DatabaseOperationExecutionService service,
			Algorithm algorithmToCreate
		)
		{
			var repositoryCollection = (RepositoryCollection)service.RepositoryCollection;
			var result = await repositoryCollection.CreateEntity(algorithmToCreate);
			return result;
		}

		public async static Task<bool> PerformCreateAlgorithmOperations(
		   this DatabaseOperationExecutionService service,
			Algorithm algorithmToCreate
		)
		{
			if (algorithmToCreate is null)
			{
				throw new DatabaseOperationException($"{nameof(algorithmToCreate)} cannot be null.");
			}

			Func<Task<bool>>[] operations =
			[
				async () => await service.CreateAlgorithm(algorithmToCreate),
			];

			try
			{
				await service.CommitAllOrRollbackAsync(operations: operations);
				return true;
			}
			catch (Exception ex)
			{
				service.Logger.LogError(ex, $"An error occurred while creating algorithm with name {algorithmToCreate.Name}.");
				return false;
			}
		}
	}
}
