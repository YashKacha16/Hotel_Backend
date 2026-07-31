using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend.Services
{
    public class RolePermissionService
    {
        private readonly AppDbContext _context;

        public RolePermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolePermission>> GetAllAsync()
        {
            return await _context.RolePermissions.AsNoTracking().ToListAsync();
        }

        public async Task<List<RolePermission>> GetByRoleAsync(string roleName)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleName.ToLower() == roleName.ToLower())
                .ToListAsync();
        }

        public async Task<RolePermission> CreateOrUpdateAsync(RolePermissionDto dto)
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => 
                    rp.RoleName.ToLower() == dto.RoleName.ToLower() && 
                    rp.ModuleName.ToLower() == dto.ModuleName.ToLower() && 
                    rp.ActionName.ToLower() == dto.ActionName.ToLower());

            if (existing == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleName = dto.RoleName,
                    ModuleName = dto.ModuleName,
                    ActionName = dto.ActionName,
                    IsAllowed = dto.IsAllowed,
                    IsLocked = dto.IsLocked
                };

                _context.RolePermissions.Add(rolePermission);
                await _context.SaveChangesAsync();
                return rolePermission;
            }

            existing.IsAllowed = dto.IsAllowed;
            existing.IsLocked = dto.IsLocked;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(string roleName)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleName.ToLower() == roleName.ToLower())
                .ToListAsync();

            if (!permissions.Any()) return false;

            _context.RolePermissions.RemoveRange(permissions);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RenameRoleAsync(string oldRoleName, string newRoleName)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleName.ToLower() == oldRoleName.ToLower())
                .ToListAsync();

            if (!permissions.Any()) return false;

            foreach (var perm in permissions)
            {
                perm.RoleName = newRoleName;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleRoleLockAsync(string roleName, bool isLocked)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleName.ToLower() == roleName.ToLower())
                .ToListAsync();

            if (!permissions.Any()) return false;

            foreach (var perm in permissions)
            {
                perm.IsLocked = isLocked;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
