using System.ComponentModel.DataAnnotations;

namespace PFEstudioJuridico.DTOs
{
    public class ActualizarContraseniaDTO
    {
        //public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string ContraseniaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NuevaContrasenia { get; set; } = string.Empty;
        public string ConfirmarClave { get; set; } = string.Empty;
        //[Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; } = string.Empty;

    }
}
