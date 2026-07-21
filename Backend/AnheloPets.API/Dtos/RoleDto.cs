using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

public class RoleDto
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleAccess { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Cantidad de usuarios con este rol asignado. Informativo, no editable.</summary>
    public int UserCount { get; set; }
}

public class CreateRoleDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El acceso del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El acceso no puede superar 100 caracteres.")]
    public string RoleAccess { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string CreatedBy { get; set; } = "admin";
}

public class UpdateRoleDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El acceso del rol es obligatorio.")]
    [StringLength(100, ErrorMessage = "El acceso no puede superar 100 caracteres.")]
    public string RoleAccess { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ModifiedBy { get; set; } = "admin";
}
