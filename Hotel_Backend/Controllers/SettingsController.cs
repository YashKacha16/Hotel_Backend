using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(AppDbContext context, IConfiguration configuration, ILogger<SettingsController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralSettings()
        {
            var settings = await _context.HotelSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Return default settings instead of 404 so the frontend form can initialize
                return Ok(new HotelSetting { 
                    Name = "", 
                    Address = "", 
                    Phone = "", 
                    Email = "",
                    Currency = "INR (₹)",
                    ServiceChargePercent = 10,
                    CgstPercent = 9,
                    SgstPercent = 9,
                    WaitlistEstimatedWaitMinutes = 22,
                    WaitlistMessage = "Based on average turnover of 48m over the last hour and 3 free tables.",
                    MinimumAdvancePercent = 0
                });
            }
            return Ok(settings);
        }

        [HttpPut("general")]
        public async Task<IActionResult> UpdateGeneralSettings([FromBody] SettingsDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _context.HotelSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new HotelSetting();
                _context.HotelSettings.Add(settings);
            }

            settings.Name = dto.Name;
            settings.Address = dto.Address;
            settings.Phone = dto.Phone;
            settings.Email = dto.Email;
            settings.Currency = dto.Currency;
            settings.ServiceChargePercent = dto.ServiceChargePercent;
            settings.CgstPercent = dto.CgstPercent;
            settings.SgstPercent = dto.SgstPercent;
            settings.WaitlistEstimatedWaitMinutes = dto.WaitlistEstimatedWaitMinutes;
            settings.WaitlistMessage = dto.WaitlistMessage;
            settings.MinimumAdvancePercent = dto.MinimumAdvancePercent;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(settings);
        }

        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo(IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg" };
            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and SVG are allowed." });
            }

            // Validate file size (2MB)
            if (logo.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds the 2MB limit." });
            }

            var basePath = _configuration.GetSection("AttachmentConfig").GetValue<string>("BasePath") ?? @"C:\hotel attachment";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = $"logo_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logo.CopyToAsync(stream);
            }

            var relativeUrl = $"/attachments/{fileName}";

            var settings = await _context.HotelSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new HotelSetting { Name = "New Property", Address = "", Phone = "", Email = "admin@hotel.com" };
                _context.HotelSettings.Add(settings);
            }
            settings.LogoUrl = relativeUrl;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { logoUrl = relativeUrl });
        }
    }
}
