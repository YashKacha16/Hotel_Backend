using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaitlistController : ControllerBase
    {
        private readonly IWaitlistService _waitlistService;

        public WaitlistController(IWaitlistService waitlistService)
        {
            _waitlistService = waitlistService;
        }

        // GET: api/waitlist
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WaitlistDto>>> GetWaitlist()
        {
            try
            {
                var list = await _waitlistService.GetAllActiveAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/waitlist
        [HttpPost]
        public async Task<ActionResult<WaitlistDto>> CreateEntry(CreateWaitlistDto dto)
        {
            try
            {
                var entry = await _waitlistService.AddAsync(dto);
                return CreatedAtAction(nameof(GetWaitlist), new { id = entry.Id }, entry);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/waitlist/{id}/status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<WaitlistDto>> UpdateStatus(int id, [FromBody] WaitlistStatus status)
        {
            try
            {
                var entry = await _waitlistService.UpdateStatusAsync(id, status);
                return Ok(entry);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/waitlist/{id}/assign
        [HttpPost("{id}/assign")]
        public async Task<ActionResult<WaitlistDto>> AssignTable(int id, [FromBody] int tableId)
        {
            try
            {
                var entry = await _waitlistService.AssignTableAsync(id, tableId);
                return Ok(entry);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
