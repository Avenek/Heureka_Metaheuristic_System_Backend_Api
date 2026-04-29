using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class CreateFitnessFunctionDtoValidator : AbstractValidator<CreateFitnessFunctionDto>
    {
        public CreateFitnessFunctionDtoValidator()
        {
            Include(new NameRules());

            RuleFor(f => f.Dimension)
                .GreaterThan(0)
                .When(f => f.Dimension is not null);

            RuleForEach(f => f.DomainPerVariable).ChildRules(domain =>
            {
                domain.RuleFor(d => d.Maximum)
                    .GreaterThan(d => d.Minimum)
                    .When(d => d.Minimum.HasValue && d.Maximum.HasValue);

                domain.RuleFor(d => d.Minimum)
                    .LessThan(d => d.Maximum)
                    .When(d => d.Minimum.HasValue && d.Maximum.HasValue);
            });
        }
    }
}
