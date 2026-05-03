using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators.Rules;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class CreateAlgorithmDtoValidator : AbstractValidator<CreateAlgorithmDto>
    {
        public CreateAlgorithmDtoValidator()
        {
            Include(new NameRules());
        }
    }
}
