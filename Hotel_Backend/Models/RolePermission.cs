using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ActionName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }

        public bool IsLocked { get; set; }
    }

    public class RolePermissionDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        public string ActionName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }

        public bool IsLocked { get; set; }
    }

    public class UpdateRolePermissionDto
    {
        public bool IsAllowed { get; set; }
    }

    public class RoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }

    public class RenameRoleDto
    {
        [Required]
        public string OldRoleName { get; set; } = string.Empty;

        [Required]
        public string NewRoleName { get; set; } = string.Empty;
    }

    public class ToggleRoleLockDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}
