using Hotel_Backend.Models;
using Hotel_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly RolePermissionService _service;

        public RolePermissionController(RolePermissionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetByRole(string roleName)
        {
            try
            {
                var data = await _service.GetByRoleAsync(roleName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] RolePermissionDto dto)
        {
            try
            {
                var data = await _service.CreateOrUpdateAsync(dto);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("role/rename")]
        public async Task<IActionResult> RenameRole([FromBody] RenameRoleDto dto)
        {
            try
            {
                var success = await _service.RenameRoleAsync(dto.OldRoleName, dto.NewRoleName);
                if (!success) return NotFound($"No permissions found for role '{dto.OldRoleName}'");
                return Ok(new { message = "Role renamed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("role/lock")]
        public async Task<IActionResult> ToggleRoleLock([FromBody] ToggleRoleLockDto dto)
        {
            try
            {
                var success = await _service.ToggleRoleLockAsync(dto.RoleName, dto.IsLocked);
                if (!success) return NotFound($"No permissions found for role '{dto.RoleName}'");
                return Ok(new { message = "Role lock status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("role/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                var success = await _service.DeleteRoleAsync(roleName);
                if (!success) return NotFound($"No permissions found for role '{roleName}'");
                return Ok(new { message = "Role permissions deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
