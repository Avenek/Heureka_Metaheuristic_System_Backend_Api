using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic
{
    public interface IPropertyRetriever<T>
    {
        Dictionary<string, object?> GetProperties(Expression<Func<T, T>> selector);
    }

}
