using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s) 
                                      || c.Email.ToLower().Contains(s) 
                                      || c.Phone.Contains(s));
            }

            var totalItems = await query.CountAsync();
            
            var clients = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clientEmails = clients.Select(c => c.Email).ToList();
            var clientNames = clients.Select(c => c.Name).ToList();

            var bookings = await _context.Bookings
                .Where(b => clientEmails.Contains(b.Email))
                .ToListAsync();

            var roomBills = await _context.RoomBills
                .Include(r => r.Booking)
                .Where(r => r.Booking != null && clientEmails.Contains(r.Booking.Email))
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => clientNames.Contains(o.CustomerName))
                .ToListAsync();

            var result = clients.Select(c => {
                var cBookings = bookings.Where(b => b.Email.Equals(c.Email, StringComparison.OrdinalIgnoreCase)).ToList();
                var cBills = roomBills.Where(r => r.Booking != null && r.Booking.Email.Equals(c.Email, StringComparison.OrdinalIgnoreCase)).ToList();
                var cOrders = orders.Where(o => o.CustomerName != null && o.CustomerName.Equals(c.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                decimal totalSpent = cBills.Sum(r => r.TotalAmount);
                if (totalSpent == 0)
                {
                    totalSpent = cBookings.Sum(b => b.AdvanceAmount);
                }

                var lastBooking = cBookings.OrderByDescending(b => b.CheckInDate).FirstOrDefault();

                return new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.CreatedAt,
                    TotalBookings = cBookings.Count,
                    TotalOrders = cOrders.Count,
                    TotalSpent = totalSpent,
                    LastCheckIn = lastBooking?.CheckInDate.ToString("yyyy-MM-dd") ?? "N/A",
                    Status = cBookings.Any(b => b.Status == "Checked-in") ? "In-House" : "Active"
                };
            }).ToList();

            return Ok(new
            {
                Items = result,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }

        // GET: api/clients/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetClientStats()
        {
            var totalClients = await _context.Clients.CountAsync();
            var activeInHouse = await _context.Bookings.Where(b => b.Status == "Checked-in").Select(b => b.Email).Distinct().CountAsync();
            
            var totalRoomRevenue = await _context.RoomBills.SumAsync(r => r.TotalAmount);
            var totalRestRevenue = await _context.RestaurantBills.SumAsync(r => r.TotalAmount);

            return Ok(new
            {
                TotalClients = totalClients,
                ActiveGuests = activeInHouse,
                TotalRevenue = totalRoomRevenue + totalRestRevenue
            });
        }

        public class UpdateClientDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        // PUT: api/clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, [FromBody] UpdateClientDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var exists = await _context.Clients.AnyAsync(c => c.Email == dto.Email && c.Id != id);
            if (exists)
            {
                return BadRequest(new { message = "Email is already registered by another client" });
            }

            client.Name = dto.Name;
            client.Email = dto.Email;
            client.Phone = dto.Phone;
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
