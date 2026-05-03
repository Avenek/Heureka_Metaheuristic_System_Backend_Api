using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators.Rules;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class UpdateFitnessFunctionDtoValidator : AbstractValidator<UpdateFitnessFunctionDto>
    {
        public UpdateFitnessFunctionDtoValidator()
        {
            Include(new NameRules());
            Include(new DomainPerVariableRule());
        }
    }
}
