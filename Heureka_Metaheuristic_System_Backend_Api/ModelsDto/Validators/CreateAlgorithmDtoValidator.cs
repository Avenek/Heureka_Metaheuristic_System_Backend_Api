using FluentValidation;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;

namespace Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Validators
{
    public class CreateAlgorithmDtoValidator : AbstractValidator<CreateAlgorithmDto>
    {
        public CreateAlgorithmDtoValidator()
        {
            Include(new AlgorithmNameRules());
        }
    }
}
