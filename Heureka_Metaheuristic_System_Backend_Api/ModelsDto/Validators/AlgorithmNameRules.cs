using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Abstraction;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class AlgorithmNameRules : AbstractValidator<IAlgorithmNameDto>
    {
        public AlgorithmNameRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(64);
        }
    }
}
