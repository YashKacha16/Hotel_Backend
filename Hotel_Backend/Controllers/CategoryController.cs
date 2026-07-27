using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hotel_Backend.Models;
using Hotel_Backend.Services;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Position = c.Position,
                IsActive = c.IsActive
            });
            return Ok(dtos);
        }

        // GET: api/category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Position = category.Position,
                IsActive = category.IsActive
            };
            return Ok(dto);
        }

        // POST: api/category
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Category
            {
                Name = dto.Name,
                Position = dto.Position,
                IsActive = dto.IsActive
            };

            var created = await _categoryService.AddCategoryAsync(category);

            var resultDto = new CategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Position = created.Position,
                IsActive = created.IsActive
            };

            return CreatedAtAction(nameof(GetCategory), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            var updated = await _categoryService.UpdateCategoryAsync(id, category);
            if (updated == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PATCH: api/category/reorder
        [HttpPatch("reorder")]
        public async Task<IActionResult> ReorderCategories([FromBody] List<CategoryPositionDto> positions)
        {
            if (positions == null || !positions.Any())
            {
                return BadRequest("No reordering positions provided.");
            }

            var success = await _categoryService.ReorderCategoriesAsync(positions);
            if (!success)
            {
                return StatusCode(500, "An error occurred while reordering categories.");
            }

            return NoContent();
        }
    }
}
