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
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET: api/Menu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems([FromQuery] int? categoryId)
        {
            var items = await _menuService.GetAllMenuItemsAsync(categoryId);
            var dtos = items.Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                CategoryId = m.CategoryId,
                Image = m.Image,
                Veg = m.Veg,
                Available = m.Available,
                Position = m.Position
            });
            return Ok(dtos);
        }

        // GET: api/Menu/grouped
        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<MenuGroupedDto>>> GetGroupedMenu()
        {
            var groupedMenu = await _menuService.GetGroupedMenuAsync();
            return Ok(groupedMenu);
        }

        // GET: api/Menu/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var dto = new MenuItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CategoryId = item.CategoryId,
                Image = item.Image,
                Veg = item.Veg,
                Available = item.Available,
                Position = item.Position
            };
            return Ok(dto);
        }

        // POST: api/Menu
        [HttpPost]
        public async Task<ActionResult<MenuItemDto>> PostMenuItem(MenuItemDto dto)
        {
            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            var menuItem = new MenuItem
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Image = dto.Image,
                Veg = dto.Veg,
                Available = dto.Available,
                Position = dto.Position
            };

            try
            {
                var created = await _menuService.AddMenuItemAsync(menuItem);
                
                var resultDto = new MenuItemDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    Description = created.Description,
                    Price = created.Price,
                    CategoryId = created.CategoryId,
                    Image = created.Image,
                    Veg = created.Veg,
                    Available = created.Available,
                    Position = created.Position
                };

                return CreatedAtAction(nameof(GetMenuItem), new { id = resultDto.Id }, resultDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Menu/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuItem(int id, MenuItemDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            var menuItem = new MenuItem
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Image = dto.Image,
                Veg = dto.Veg,
                Available = dto.Available,
                Position = dto.Position
            };

            try
            {
                var updated = await _menuService.UpdateMenuItemAsync(id, menuItem);
                if (updated == null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var deleted = await _menuService.DeleteMenuItemAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/Menu/5/availability
        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var success = await _menuService.ToggleAvailabilityAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/Menu/reorder
        [HttpPatch("reorder")]
        public async Task<IActionResult> ReorderMenuItems([FromBody] List<CategoryPositionDto> positions)
        {
            if (positions == null || !positions.Any())
            {
                return BadRequest("No reordering positions provided.");
            }

            var success = await _menuService.ReorderMenuItemsAsync(positions);
            if (!success)
            {
                return StatusCode(500, "An error occurred while reordering menu items.");
            }

            return NoContent();
        }

        // POST: api/Menu/5/image
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(int id, Microsoft.AspNetCore.Http.IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var result = await _menuService.UploadImageAsync(id, image);
            if (!result)
            {
                return NotFound("Menu item not found.");
            }

            return Ok(new { message = "Image uploaded successfully" });
        }
    }
}
