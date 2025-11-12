using Microsoft.AspNetCore.Mvc;
using MindfulMe.API.DTOs;
using MindfulMe.API.Models;
using MindfulMe.API.Services;

namespace MindfulMe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeditationController : ControllerBase
    {
        private readonly IMeditationService _meditationService;

        public MeditationController(IMeditationService meditationService)
        {
            _meditationService = meditationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<MeditationSession>>> GetUserSessions(int userId)
        {
            var sessions = await _meditationService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeditationSession>> GetSession(int id)
        {
            var session = await _meditationService.GetSessionByIdAsync(id);
            
            if (session == null)
                return NotFound(new { Message = "Session not found" });
            
            return Ok(session);
        }

        [HttpPost]
        public async Task<ActionResult<MeditationSession>> CreateSession([FromBody] CreateMeditationSessionRequest request)
        {
            var session = await _meditationService.CreateSessionAsync(request);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSession(int id, [FromBody] MeditationSession session)
        {
            if (id != session.Id)
                return BadRequest(new { Message = "ID mismatch" });
            
            var success = await _meditationService.UpdateSessionAsync(session);
            
            if (!success)
                return NotFound(new { Message = "Session not found or unauthorized" });
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSession(int id, [FromQuery] int userId)
        {
            var success = await _meditationService.DeleteSessionAsync(id, userId);
            
            if (!success)
                return NotFound(new { Message = "Session not found or unauthorized" });
            
            return NoContent();
        }
    }
}