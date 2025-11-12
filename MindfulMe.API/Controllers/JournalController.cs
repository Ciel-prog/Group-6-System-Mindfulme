using Microsoft.AspNetCore.Mvc;
using MindfulMe.API.DTOs;
using MindfulMe.API.Models;
using MindfulMe.API.Services;

namespace MindfulMe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController : ControllerBase
    {
        private readonly IJournalService _journalService;

        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<JournalEntry>>> GetUserEntries(int userId)
        {
            var entries = await _journalService.GetUserEntriesAsync(userId);
            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JournalEntry>> GetEntry(int id)
        {
            var entry = await _journalService.GetEntryByIdAsync(id);

            if (entry == null)
                return NotFound(new { Message = "Entry not found" });

            return Ok(entry);
        }

        [HttpPost]
        public async Task<ActionResult<JournalEntry>> CreateEntry([FromBody] CreateJournalEntryRequest request)
        {
            var entry = await _journalService.CreateEntryAsync(request);
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEntry(int id, [FromBody] JournalEntry entry)
        {
            if (id != entry.Id)
                return BadRequest(new { Message = "ID mismatch" });

            var success = await _journalService.UpdateEntryAsync(entry);

            if (!success)
                return NotFound(new { Message = "Entry not found or unauthorized" });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEntry(int id, [FromQuery] int userId)
        {
            var success = await _journalService.DeleteEntryAsync(id, userId);

            if (!success)
                return NotFound(new { Message = "Entry not found or unauthorized" });

            return NoContent();
        }
    }
}