using Heureka_Metaheuristic_System_Backend_Api.Database;

namespace Heureka_Metaheuristic_System_Backend_Api.Contracts
{
    public interface IRepositoryCollection : IDisposable, IAsyncDisposable
    {
        DatabaseContext Context { get; }
    }
}
