using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators.Rules
{
    public class DomainPerVariableRule : AbstractValidator<IDomainPerVariableDto>
    {
        public DomainPerVariableRule()
        {
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
