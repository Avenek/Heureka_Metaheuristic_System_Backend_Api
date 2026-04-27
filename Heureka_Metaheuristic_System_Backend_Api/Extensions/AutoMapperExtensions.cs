using AutoMapper;

namespace Heureka_Metaheuristic_System_Backend_Api.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreNullOrEmpty<TSource, TDestination>(
         this IMappingExpression<TSource, TDestination> expression)
        {
            expression.ForAllMembers(opt =>
            {
                opt.Condition((src, dest, srcMember) =>
                    srcMember != null &&
                    !(srcMember is string s && string.IsNullOrWhiteSpace(s)));
            });

            return expression;
        }
    }
}
