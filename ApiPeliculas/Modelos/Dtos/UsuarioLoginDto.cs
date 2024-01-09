using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos.Dtos
{
    public class UsuarioLoginDto
    {
        [Required(ErrorMessage ="El usuario es obligatorio")]
        public string NombreUSuario { get; set; }

        [Required(ErrorMessage = "El password es obligatorio")]
        public string Password { get; set; }

        public string Role { get; set; }
    }
}
