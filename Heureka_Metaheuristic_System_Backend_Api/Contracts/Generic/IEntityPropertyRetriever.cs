using Heureka_Metaheuristic_System_Backend_Api.Database;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic
{
    public interface IEntityPropertyRetriever<T> : IPropertyRetriever<T> where T : IEntity
    {
        ISet<string> GetEntityProperties(DatabaseContext context);

        ISet<string> GetEntityReadOnlyProperties(DatabaseContext context);
    }

}
