using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class DatabaseRepository<TEntity> : IDatabaseRepository<TEntity> where TEntity : class, IEntity
    {
        public DatabaseRepository(DatabaseContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public DatabaseContext Context { get; }

        public DbSet<TEntity> DbSet { get; }

        public IQueryable<TEntity> Query() => DbSet;

        public IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = ""
        )
        {
            IQueryable<TEntity> query = DbSet;

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, static (current, includeProperty) => current.Include(includeProperty));

            return orderBy is not null ? orderBy(query).ToList() : query.ToList();
        }

        IQueryable<object> IRepository.Query() => Query();

        public IEnumerable<object> Get(
            Expression<Func<object, bool>>? filter,
            Func<IQueryable<object>, IOrderedQueryable<object>>? orderBy,
            string includeProperties
        ) =>
            Get(
                filter as Expression<Func<TEntity, bool>>,
                orderBy as Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>,
                includeProperties
            );

        async Task<object?> IRepository.GetById(object? id) => await GetById(id);

        public void Insert(object entity) => Insert(entity as TEntity);

        public async Task<TEntity> GetById(object? id)
        {
            var entity = await DbSet.FindAsync(id);
            if(entity is null)
            {
                throw new NotFoundException($"Entity of type {typeof(TEntity).Name} with id {id} not found.");
            }

            return entity;
        }

        public void Insert(TEntity? entity)
        {
            if (entity is null)
            {
                return;
            }

            DbSet.Add(entity);
        }

        public void Delete(object? id)
        {
            var entityToDelete = DbSet.Find(id);
            Delete(entityToDelete);
        }

        public void Delete(TEntity? entityToDelete)
        {
            if (entityToDelete is null)
            {
                return;
            }

            if (Context.Entry(entityToDelete).State == EntityState.Detached)
            {
                DbSet.Attach(entityToDelete);
            }

            DbSet.Remove(entityToDelete);
        }

        public void Update(object entity) => Update(entity as TEntity);

        public void Update(TEntity? entityToUpdate)
        {
            if (entityToUpdate is null)
            {
                return;
            }

            DbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}
