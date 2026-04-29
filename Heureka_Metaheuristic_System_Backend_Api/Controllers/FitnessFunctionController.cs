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
    }
}