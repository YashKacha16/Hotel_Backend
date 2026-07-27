using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;

namespace Hotel_Backend.Controllers
{
    [ApiController]
    [Route("api/table-category")]
    public class TableCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TableCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableCategory>>> GetAll()
        {
            return await _context.TableCategories
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<TableCategory>> Create(TableCategory category)
        {
            _context.TableCategories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TableCategory category)
        {
            if (id != category.Id) return BadRequest();
            var existing = await _context.TableCategories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = category.Name;
            existing.Position = category.Position;
            existing.IsActive = category.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.TableCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.TableCategories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
