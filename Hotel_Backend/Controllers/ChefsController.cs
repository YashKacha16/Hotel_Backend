using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Microsoft.Extensions.Configuration;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChefsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ChefsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetChefs()
        {
            var chefs = await _context.Chefs.OrderBy(c => c.CreatedAt).ToListAsync();
            return Ok(chefs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChef(int id)
        {
            var chef = await _context.Chefs.FindAsync(id);
            if (chef == null)
            {
                return NotFound(new { message = "Chef not found" });
            }
            return Ok(chef);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChef([FromBody] Chef chef)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            chef.CreatedAt = DateTime.UtcNow;
            _context.Chefs.Add(chef);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChef), new { id = chef.Id }, chef);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChef(int id, [FromBody] Chef chefInput)
        {
            if (id != chefInput.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var chef = await _context.Chefs.FindAsync(id);
            if (chef == null)
            {
                return NotFound(new { message = "Chef not found" });
            }

            chef.Name = chefInput.Name;
            chef.Role = chefInput.Role;
            chef.Description = chefInput.Description;
            chef.ImageUrl = chefInput.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(chef);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChef(int id)
        {
            var chef = await _context.Chefs.FindAsync(id);
            if (chef == null)
            {
                return NotFound(new { message = "Chef not found" });
            }

            _context.Chefs.Remove(chef);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chef deleted successfully" });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, JPEG, PNG, SVG, WEBP, GIF." });
            }

            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds the 2MB limit." });
            }

            var basePath = _configuration.GetSection("AttachmentConfig").GetValue<string>("BasePath") ?? @"C:\hotel attachment";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = $"chef_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/attachments/{fileName}";
            return Ok(new { imageUrl = relativeUrl });
        }
    }
}
