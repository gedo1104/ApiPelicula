﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos
{
    public class Pelicula
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Duracion { get; set; }

        public string? RutaImagen { get; set; }   

        public string? RutaLocalImagen { get; set; }

        public enum TipoClasificacion { Siente, Trece, Diecisies, dieciocho}

        public TipoClasificacion Clasificaion { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }


    }
}
