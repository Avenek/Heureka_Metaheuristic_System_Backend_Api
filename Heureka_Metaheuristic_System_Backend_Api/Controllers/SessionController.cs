using Heureka_Metaheuristic_System_Backend_Api.ModelsDto.Requests.Sessions;
using Heureka_Metaheuristic_System_Backend_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Heureka_Metaheuristic_System_Backend_Api.Controllers
{
    [Route("api/v1/sessions")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService sessionService;

        public SessionController(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] uint? state)
        {
            var sessions = await sessionService.GetAll(state);
            return Ok(sessions);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteById([FromRoute] uint id)
        {
            await sessionService.DeleteById(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto createSessionDto, CancellationToken cancellationToken)
        {
            await sessionService.CreateSession(createSessionDto, cancellationToken);
            return Ok();
        }
    }
}
