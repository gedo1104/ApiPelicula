﻿using ApiPeliculas.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //! agregar los modelos aaqui

        public DbSet<Categoria> Categoria { get; set; }

        public DbSet<Pelicula> Pelicula { get; set; }

        public DbSet<Usuario> Usuario { get; set; }



    }
}
