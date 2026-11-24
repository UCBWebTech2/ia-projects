using System.ComponentModel.DataAnnotations;

namespace PFEstudioJuridico.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string correo {  get; set; } = string.Empty;
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        //[MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string clave { get; set; } = string.Empty;
    }
}
