using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Runtime.CompilerServices;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class DatabaseOperationExecutionService(
    IRepositoryCollection repositoryCollection,
    IMappingService mappingService,
    ILogger<DatabaseOperationExecutionService>? logger
    ) : IDisposable, IAsyncDisposable, IDatabaseOperationExecutionService
    {
        public async ValueTask DisposeAsync()
        {
            await repositoryCollection.DisposeAsync();
        }

        public void Dispose()
        {
            repositoryCollection.Dispose();
        }

        public TMappingService GetMappingService<TMappingService>() where TMappingService : IMappingService =>
            (TMappingService)mappingService;

        public ILogger Logger => logger;
        public IRepositoryCollection RepositoryCollection => repositoryCollection;

        public virtual async Task CommitAllOrRollbackAsync(
            [CallerMemberName] string caller = "",
            params Func<Task<bool>>[] operations)
        {
            var context = RepositoryCollection.Context;
            var retries = 3;
            var delayMiliseconds = 100;

            for (int attempt = 0; attempt < retries; attempt++)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var operation in operations)
                    {
                        if (!await operation())
                        {
                            throw new DatabaseOperationException("Operations on database failed.");
                        }
                    }

                    await transaction.CommitAsync();
                    return;
                }
                catch (Exception e) when (IsDeadlockException(e))
                {
                    await transaction.RollbackAsync();
                    if (attempt == retries - 1)
                    {
                        throw;
                    }
                        
                    await Task.Delay(delayMiliseconds * (attempt + 1));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private static bool IsDeadlockException(Exception ex)
        {
            return ex is MySqlException mySqlEx && mySqlEx.Number == 1213;
        }
    }
}
