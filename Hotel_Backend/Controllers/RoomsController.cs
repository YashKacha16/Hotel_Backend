using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms([FromQuery] int? categoryId, [FromQuery] string? status, [FromQuery] string? floor)
        {
            var query = _context.Rooms
                .Include(r => r.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(floor))
            {
                query = query.Where(r => r.Floor == floor);
            }

            var rooms = await query.ToListAsync();

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Number = r.Number,
                CategoryId = r.CategoryId,
                Category = r.Category != null ? new RoomCategoryDto
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name,
                    BasePrice = r.Category.BasePrice,
                    Currency = r.Category.Currency,
                    SeasonalPricingEnabled = r.Category.SeasonalPricingEnabled,
                    IsActive = r.Category.IsActive
                } : null,
                Floor = r.Floor,
                Capacity = r.Capacity,
                BasePrice = r.BasePrice,
                Status = r.Status,
                Amenities = r.Amenities,
                Images = r.Images,
                Description = r.Description
            }).ToList();
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var r = await _context.Rooms
                .Include(ro => ro.Category)
                .FirstOrDefaultAsync(ro => ro.Id == id);

            if (r == null)
            {
                return NotFound();
            }

            return new RoomDto
            {
                Id = r.Id,
                Number = r.Number,
                CategoryId = r.CategoryId,
                Category = r.Category != null ? new RoomCategoryDto
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name,
                    BasePrice = r.Category.BasePrice,
                    Currency = r.Category.Currency,
                    SeasonalPricingEnabled = r.Category.SeasonalPricingEnabled,
                    IsActive = r.Category.IsActive
                } : null,
                Floor = r.Floor,
                Capacity = r.Capacity,
                BasePrice = r.BasePrice,
                Status = r.Status,
                Amenities = r.Amenities,
                Images = r.Images,
                Description = r.Description
            };
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<ActionResult<RoomDto>> PostRoom(CreateRoomDto dto)
        {
            // Validate capacity
            if (dto.Capacity <= 0) return BadRequest("Capacity must be greater than 0");
            if (dto.BasePrice < 0) return BadRequest("Base price cannot be negative");

            // Validate duplicate room number
            var existing = await _context.Rooms.FirstOrDefaultAsync(r => r.Number == dto.Number);
            if (existing != null)
            {
                return Conflict(new { message = $"Room number {dto.Number} already exists." });
            }

            var room = new Room
            {
                Number = dto.Number,
                CategoryId = dto.CategoryId,
                Floor = dto.Floor,
                Capacity = dto.Capacity,
                BasePrice = dto.BasePrice,
                Status = dto.Status,
                Amenities = dto.Amenities,
                Images = dto.Images,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.Id }, new RoomDto { Id = room.Id, Number = room.Number });
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, UpdateRoomDto dto)
        {
            if (dto.Capacity <= 0) return BadRequest("Capacity must be greater than 0");
            if (dto.BasePrice < 0) return BadRequest("Base price cannot be negative");

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Check for duplicate number if changing number
            if (room.Number != dto.Number)
            {
                var existing = await _context.Rooms.FirstOrDefaultAsync(r => r.Number == dto.Number);
                if (existing != null)
                {
                    return Conflict(new { message = $"Room number {dto.Number} already exists." });
                }
            }

            room.Number = dto.Number;
            room.CategoryId = dto.CategoryId;
            room.Floor = dto.Floor;
            room.Capacity = dto.Capacity;
            room.BasePrice = dto.BasePrice;
            room.Status = dto.Status;
            room.Amenities = dto.Amenities;
            room.Images = dto.Images;
            room.Description = dto.Description;
            room.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // TODO: In future, check for active or future bookings here and return BadRequest/Conflict
            // e.g. if (_context.Bookings.Any(b => b.RoomId == id && b.Status != "Completed")) return BadRequest("Cannot delete room with active bookings");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}
