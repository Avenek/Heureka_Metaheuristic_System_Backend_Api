using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts
{
    public interface IRepository
    {
        public IQueryable<object> Query();

        public IEnumerable<object> Get(
            Expression<Func<object, bool>>? filter,
            Func<IQueryable<object>, IOrderedQueryable<object>>? orderBy,
            string includeProperties
        );

        public object? GetById(object? id);

        public void Insert(object entity);

        public void Delete(object? id);

        public void Update(object entity);
    }
}
