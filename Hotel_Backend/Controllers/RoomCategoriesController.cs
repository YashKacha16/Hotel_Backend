using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // ── Room Categories CRUD ────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomCategoryDto>>> GetAll()
        {
            var categories = await _context.RoomCategories
                .Include(rc => rc.SeasonalRules)
                .OrderBy(rc => rc.Name)
                .ToListAsync();

            return Ok(categories.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomCategoryDto>> GetById(int id)
        {
            var category = await _context.RoomCategories
                .Include(rc => rc.SeasonalRules)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (category == null) return NotFound();
            return Ok(MapToDto(category));
        }

        [HttpPost]
        public async Task<ActionResult<RoomCategoryDto>> Create([FromBody] CreateRoomCategoryDto dto)
        {
            // Check unique name
            var exists = await _context.RoomCategories.AnyAsync(rc => rc.Name == dto.Name);
            if (exists) return BadRequest(new { message = $"Category '{dto.Name}' already exists." });

            var category = new RoomCategory
            {
                Name = dto.Name,
                BasePrice = dto.BasePrice,
                Currency = dto.Currency,
                SeasonalPricingEnabled = dto.SeasonalPricingEnabled,
                IsActive = dto.IsActive,
                Capacity = dto.Capacity,
                Amenities = dto.Amenities,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RoomCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, MapToDto(category));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoomCategoryDto>> Update(int id, [FromBody] UpdateRoomCategoryDto dto)
        {
            var category = await _context.RoomCategories
                .Include(rc => rc.SeasonalRules)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (category == null) return NotFound();

            // Check unique name (exclude self)
            var nameExists = await _context.RoomCategories.AnyAsync(rc => rc.Name == dto.Name && rc.Id != id);
            if (nameExists) return BadRequest(new { message = $"Category '{dto.Name}' already exists." });

            category.Name = dto.Name;
            category.BasePrice = dto.BasePrice;
            category.Currency = dto.Currency;
            category.SeasonalPricingEnabled = dto.SeasonalPricingEnabled;
            category.IsActive = dto.IsActive;
            category.Capacity = dto.Capacity;
            category.Amenities = dto.Amenities;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(category));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.RoomCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.RoomCategories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Seasonal Rules CRUD ─────────────────────────────────

        [HttpGet("{id}/seasonal-rules")]
        public async Task<ActionResult<IEnumerable<SeasonalRuleDto>>> GetSeasonalRules(int id)
        {
            var categoryExists = await _context.RoomCategories.AnyAsync(rc => rc.Id == id);
            if (!categoryExists) return NotFound(new { message = "Category not found." });

            var rules = await _context.SeasonalRules
                .Where(sr => sr.RoomCategoryId == id)
                .OrderBy(sr => sr.StartDate)
                .ToListAsync();

            return Ok(rules.Select(MapRuleToDto));
        }

        [HttpPost("{id}/seasonal-rules")]
        public async Task<ActionResult<SeasonalRuleDto>> CreateSeasonalRule(int id, [FromBody] CreateSeasonalRuleDto dto)
        {
            var categoryExists = await _context.RoomCategories.AnyAsync(rc => rc.Id == id);
            if (!categoryExists) return NotFound(new { message = "Category not found." });

            var rule = new SeasonalRule
            {
                RoomCategoryId = id,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsRecurring = dto.IsRecurring,
                DaysOfWeek = dto.DaysOfWeek,
                PriceModifierPercent = dto.PriceModifierPercent,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.SeasonalRules.Add(rule);
            await _context.SaveChangesAsync();

            return Ok(MapRuleToDto(rule));
        }

        [HttpPut("{id}/seasonal-rules/{ruleId}")]
        public async Task<ActionResult<SeasonalRuleDto>> UpdateSeasonalRule(int id, int ruleId, [FromBody] UpdateSeasonalRuleDto dto)
        {
            var rule = await _context.SeasonalRules
                .FirstOrDefaultAsync(sr => sr.Id == ruleId && sr.RoomCategoryId == id);

            if (rule == null) return NotFound(new { message = "Rule not found." });

            rule.Name = dto.Name;
            rule.StartDate = dto.StartDate;
            rule.EndDate = dto.EndDate;
            rule.IsRecurring = dto.IsRecurring;
            rule.DaysOfWeek = dto.DaysOfWeek;
            rule.PriceModifierPercent = dto.PriceModifierPercent;
            rule.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(MapRuleToDto(rule));
        }

        [HttpDelete("{id}/seasonal-rules/{ruleId}")]
        public async Task<IActionResult> DeleteSeasonalRule(int id, int ruleId)
        {
            var rule = await _context.SeasonalRules
                .FirstOrDefaultAsync(sr => sr.Id == ruleId && sr.RoomCategoryId == id);

            if (rule == null) return NotFound();

            _context.SeasonalRules.Remove(rule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Mapping helpers ─────────────────────────────────────

        private static RoomCategoryDto MapToDto(RoomCategory rc) => new()
        {
            Id = rc.Id,
            Name = rc.Name,
            BasePrice = rc.BasePrice,
            Currency = rc.Currency,
            SeasonalPricingEnabled = rc.SeasonalPricingEnabled,
            IsActive = rc.IsActive,
            Capacity = rc.Capacity,
            Amenities = rc.Amenities,
            ImageUrl = rc.ImageUrl,
            SeasonalRuleCount = rc.SeasonalRules?.Count ?? 0
        };

        private static SeasonalRuleDto MapRuleToDto(SeasonalRule sr) => new()
        {
            Id = sr.Id,
            RoomCategoryId = sr.RoomCategoryId,
            Name = sr.Name,
            StartDate = sr.StartDate,
            EndDate = sr.EndDate,
            IsRecurring = sr.IsRecurring,
            DaysOfWeek = sr.DaysOfWeek,
            PriceModifierPercent = sr.PriceModifierPercent,
            IsActive = sr.IsActive
        };
    }
}
