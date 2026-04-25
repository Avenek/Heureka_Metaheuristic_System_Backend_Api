using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Microsoft.EntityFrameworkCore;
using NLog.Config;
using System.Security.Principal;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class RepositoryCollection(DatabaseContext context, IRepositoryFactory factory) : IRepositoryCollection, IDisposable
    {
        private readonly Dictionary<Type, IRepository> repositories = [];
        public DatabaseContext Context => context;

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
        }

        public void Dispose()
        {
            if (context is IDisposable contextDisposable)
            {
                contextDisposable.Dispose();
            }
            else
            {
                _ = context.DisposeAsync().AsTask();
            }
        }

    public DatabaseRepository<TEntity> Get<TEntity>() where TEntity : class, IEntity
    {
            if (repositories.TryGetValue(typeof(TEntity), out var repository))
            {
                return (DatabaseRepository<TEntity>)repository;
            }

            var value = factory.Create<TEntity>(context)!;

            repositories[typeof(TEntity)] = value;

            return value;
        }
    }
}
