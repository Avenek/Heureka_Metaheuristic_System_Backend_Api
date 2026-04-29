using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Extensions
{
    public static class ClassExtensions
    {
        public static T ThrowIfNull<T>(this T? entity, string message) where T : class
        {
            if (entity is null)
            {
                throw new NotFoundException(message);
            }
                
            return entity;
        }
    }
}
