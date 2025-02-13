using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos.Dtos
{
    public class CrearPeliculaDto
    {

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        public string? RutaImagen { get; set; }

        public IFormFile Image { get; set; }


        [Required(ErrorMessage = "La descripcion es requerido")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La duracion es requerido")]
        public int Duracion { get; set; }

        public enum TipoClasificacion { Siete, Trece, Diecisies, dieciocho }

        public TipoClasificacion Clasificaion { get; set; }

        public DateTime? FechaCreacion { get; set; }
        public int CategoriaId { get; set; }

    }
}
