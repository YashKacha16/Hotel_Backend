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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // For a simple session-less API, logout just returns success.
            // If using cookies, you would clear the cookie here.
            return Ok(new { message = "Logout successful" });
        }
    }
}
