using Heureka_Metaheuristic_System_Backend_Api.ModelsDto;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Algorithms;
using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Responses.Algorithmss;
using Heureka_Metaheuristic_System_Backend_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Heureka_Metaheuristic_System_Backend_Api.Controllers
{
    [Route("api/v1/algorithms")]
    [ApiController]
    public class AlgorithmController : ControllerBase
    {
        private readonly IAlgorithmService algorithmService;

        public AlgorithmController(IAlgorithmService algorithmService)
        {
            this.algorithmService = algorithmService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var algorithms = await algorithmService.GetAll();
            return Ok(algorithms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById([FromRoute] uint id)
        {
            var algorithm = await algorithmService.GetById(id);
            return Ok(algorithm);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAlgorithm([FromForm] CreateAlgorithmDto algorithmToCreate, IFormFile file)
        {
            var createdAlgorithm = await algorithmService.CreateAlgorithm(algorithmToCreate, file);
            return Created($"api/v1/algorithms/{createdAlgorithm.Id}", createdAlgorithm);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateById([FromRoute] uint id, [FromBody] UpdateAlgorithmDto updatedAlgorithmDto)
        {
            var updatedAlgorithm = await algorithmService.UpdateById(id, updatedAlgorithmDto);
            return Ok(updatedAlgorithm);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteById([FromRoute] uint id)
        {
            await algorithmService.DeleteById(id);
            return NoContent();
        }
    }
}
