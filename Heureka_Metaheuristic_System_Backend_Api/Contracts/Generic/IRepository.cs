using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic
{
    public interface IRepository<T> : IRepository
    {
        public new IQueryable<T> Query();

        public IEnumerable<T> Get(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
            string includeProperties
        );

        public new Task<T?> GetById(object? id);

        public void Insert(T entity);

        public new void Delete(object? id);

        public void Delete(T? entityToDelete);

        public void Update(T entity);
    }
}
