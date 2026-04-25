using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators;
using System.Reflection;

namespace Heureka_Metaheuristic_System_Backend_Api.Registrars.Extensions
{
    public static class ValidatorExtension
    {
        public static void AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var baseNamespace = typeof(AbstractValidator).Namespace;

            var validatorTypes = assembly.GetTypes().Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.Namespace != null &&
                type.Namespace.StartsWith(baseNamespace));

            foreach (var validatorType in validatorTypes)
            {
                var genericValidatorInterface = typeof(IValidator<>).MakeGenericType(validatorType);
                services.AddScoped(genericValidatorInterface, validatorType);
            }
        }
    }
}
