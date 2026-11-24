using System.ComponentModel.DataAnnotations;

namespace PFEstudioJuridico.DTOs
{
    public class RegistroUsuarioDTO
    {
        public int IdUsuario { get; set; }
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede tener más de 50 caracteres.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [RolValido(ErrorMessage = "Seleccione un rol válido.")]
        public string Rol { get; set; } = string.Empty;
    }
    public class RolValidoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var rol = value?.ToString();
            if (string.IsNullOrWhiteSpace(rol) || rol == "Seleccione")
            {
                return new ValidationResult(ErrorMessage ?? "Seleccione un rol válido.");
            }
            return ValidationResult.Success;
        }
    }
}
