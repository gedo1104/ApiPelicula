﻿using Microsoft.AspNetCore.Identity;

namespace ApiPeliculas.Modelos
{
    public class AppUsuario : IdentityUser
    {
        //agregar campos personalizados
        public string Nombre { get; set; }
    }
}
