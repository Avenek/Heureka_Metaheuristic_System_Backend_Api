using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.FitnessFunctions;
using Heureka_Metaheuristic_System_Backend_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Heureka_Metaheuristic_System_Backend_Api.Controllers
{
    [Route("api/v1/fitness-functions")]
    [ApiController]
    public class FitnessFunctionController : ControllerBase
    {
        private readonly IFitnessFunctionService fitnessFunctionService;

        public FitnessFunctionController(IFitnessFunctionService fitnessFunctionService)
        {
            this.fitnessFunctionService = fitnessFunctionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var fitnessFunctions = await fitnessFunctionService.GetAll();
            return Ok(fitnessFunctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById([FromRoute] uint id)
        {
            var fitnessFunction = await fitnessFunctionService.GetById(id);
            return Ok(fitnessFunction);
        }

        [HttpPost]
        public async Task<ActionResult> CreateFitnessFunction([FromForm] CreateFitnessFunctionDto fitnessFunctionToCreate, IFormFile file)
        {
            var createdFitnessFunction = await fitnessFunctionService.CreateFitnessFunction(fitnessFunctionToCreate, file);
            return CreatedAtAction(nameof(GetById), new { id = createdFitnessFunction.Id }, createdFitnessFunction);
        }
    }
}