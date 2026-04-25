using Heureka_Metaheuristic_System_Backend_Api.Contracts;
using Heureka_Metaheuristic_System_Backend_Api.Contracts.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace Heureka_Metaheuristic_System_Backend_Api.Database
{
    public class PropertyRetriever<TEntity> : IEntityPropertyRetriever<TEntity> where TEntity : class, IEntity
    {
        private IDictionary<string, Func<TEntity, object?>>? propertyAccessors;

        private IDictionary<string, Func<TEntity, object?>> PropertyAccessors => propertyAccessors ??= PropertyRetriever<TEntity>.GetPropertyAccessors();

        public PropertyRetriever()
        {

        }

        public ISet<string> GetEntityProperties(DatabaseContext context) => context.Model
                .FindEntityType(typeof(TEntity))?
                .GetProperties()
                .Select(p => p.Name).ToHashSet() ?? [];

        public ISet<string> GetEntityReadOnlyProperties(DatabaseContext context) => context.Model
                .FindEntityType(typeof(TEntity))?
                .GetProperties()
                .Where(p => p.IsPrimaryKey() || p.IsForeignKey() || p.IsConcurrencyToken || p.ValueGenerated == ValueGenerated.OnAddOrUpdate || p.ValueGenerated == ValueGenerated.OnAdd)
                .Select(p => p.Name)
                .ToHashSet() ?? [];

        public Dictionary<string, object?> GetProperties(Expression<Func<TEntity, TEntity>> selector)
        {
            return selector.Body switch
            {
                MemberInitExpression memberInitExpression => GetInitializedProperties(memberInitExpression),
                MemberExpression memberExpression when memberExpression.Expression is ConstantExpression => GetEvaluatedProperties(selector.Compile().Invoke(default!)),
                NewExpression => GetEvaluatedProperties(selector.Compile().Invoke(default!)),
                _ => []
            };
        }

        private Dictionary<string, object?> GetInitializedProperties(MemberInitExpression memberInitExpression)
        {
            var bindings = memberInitExpression.Bindings.OfType<MemberAssignment>().ToArray();
            var result = new Dictionary<string, object?>(bindings.Length);

            if (bindings.Any(b => b.Expression is not ConstantExpression))
            {
                var instance = (TEntity)Expression.Lambda(memberInitExpression).Compile().DynamicInvoke()!;

                foreach (var binding in bindings)
                {
                    result[binding.Member.Name] = PropertyAccessors[binding.Member.Name](instance);
                }

                return result;
            }

            foreach (var binding in bindings)
            {
                result[binding.Member.Name] = binding.Expression;
            }

            return result;
        }

        private static Dictionary<string, Func<TEntity, object?>> GetPropertyAccessors()
        {
            var properties = typeof(TEntity).GetProperties();
            var propertyAccessors = new Dictionary<string, Func<TEntity, object?>>(properties.Length);

            foreach (var property in properties)
            {
                var propertyGetMethod = property.GetMethod;

                if (propertyGetMethod is null)
                {
                    continue;
                }

                var parameterExpression = Expression.Parameter(typeof(TEntity));
                var propertyExpression = Expression.Property(parameterExpression, propertyGetMethod);
                var convertExpression = Expression.Convert(propertyExpression, typeof(object));
                var lambdaExpression = Expression.Lambda<Func<TEntity, object>>(convertExpression, parameterExpression);

                propertyAccessors[property.Name] = lambdaExpression.Compile();
            }

            return propertyAccessors;
        }

        private Dictionary<string, object?> GetEvaluatedProperties(TEntity entity) => PropertyAccessors.ToDictionary(k => k.Key, v => v.Value(entity));
    }
}
