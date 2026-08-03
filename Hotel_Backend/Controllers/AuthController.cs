using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class ClientRegisterDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == loginDto.Email && e.Password == loginDto.Password);

            if (employee == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Simple login without JWT
            return Ok(new 
            { 
                message = "Login successful",
                employee = new { employee.Id, employee.Name, employee.Email, employee.Role, employee.PhotoPath } 
            });
        }

        [HttpPost("client/login")]
        public async Task<IActionResult> ClientLogin([FromBody] LoginDto loginDto)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == loginDto.Email && c.Password == loginDto.Password);

            if (client == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new 
            { 
                message = "Login successful",
                client = new { client.Id, client.Name, client.Email, client.Phone } 
            });
        }

        [HttpPost("client/register")]
        public async Task<IActionResult> ClientRegister([FromBody] ClientRegisterDto registerDto)
        {
            var exists = await _context.Clients.AnyAsync(c => c.Email == registerDto.Email);
            if (exists)
            {
                return BadRequest(new { message = "Email is already registered" });
            }

            var client = new Client
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                Password = registerDto.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Registration successful",
                client = new { client.Id, client.Name, client.Email, client.Phone } 
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // For a simple session-less API, logout just returns success.
            // If using cookies, you would clear the cookie here.
            return Ok(new { message = "Logout successful" });
        }
    }
}
