using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators.Rules
{
    public class NameRules : AbstractValidator<INameDto>
    {
        public NameRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(64);
        }
    }
}
