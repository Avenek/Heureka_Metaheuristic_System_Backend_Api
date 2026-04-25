using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Heureka_Metaheuristic_System_Backend_Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AlgorithmController : ControllerBase
    {
        private readonly IAlgorithmService algorithmService;

        public AlgorithmController(IAlgorithmService algorithmService)
        {
            this.algorithmService = algorithmService;
        }

        [HttpGet]
        public ActionResult GetAll()
        {
            var algorithms = algorithmService.GetAll();
            return Ok(algorithms);
        }
    }
}
