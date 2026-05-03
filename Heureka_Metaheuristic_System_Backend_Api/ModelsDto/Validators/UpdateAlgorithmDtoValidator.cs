using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators.Rules;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class UpdateAlgorithmDtoValidator : AbstractValidator<UpdateAlgorithmDto>
    {
        public UpdateAlgorithmDtoValidator()
        {
            Include(new NameRules());
        }
    }
}
