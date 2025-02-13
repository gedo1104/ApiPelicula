
using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db; //instancio 
        private string claveSecreta;
        private readonly UserManager<AppUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly IMapper _mapper;

        public UsuarioRepositorio(ApplicationDbContext db, IConfiguration config, 
                UserManager<AppUsuario> userManager, RoleManager<IdentityRole> rolManager, IMapper mapper)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:token");
            _userManager = userManager;
            _rolManager = rolManager;
            _mapper = mapper;

        }
        public AppUsuario GetUsuario(string usuarioId)
        {

            return _db.AppUsuario.FirstOrDefault(u => u.Id == usuarioId);
        }

        public ICollection<AppUsuario> GetUsuarios()
        {
            return _db.AppUsuario.OrderBy(u => u.UserName).ToList(); 
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuariodb = _db.AppUsuario.FirstOrDefault(u => u.UserName == usuario);

            if (usuariodb == null)
            {
                return true;
            }

            return false;
            
        }

        public  async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {
            //recordar que la clave debe de ser mayuscula, numeros, letras, simbolo, min 6 caracteres ejem: Admin123*

            AppUsuario usuario = new AppUsuario()
            {
                UserName = usuarioRegistroDto.NombreUSuario,
                Email = usuarioRegistroDto.NombreUSuario,
                NormalizedEmail = usuarioRegistroDto.NombreUSuario.ToUpper(),
                Nombre = usuarioRegistroDto.Nombre
                
            };

            var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);

            if (result.Succeeded)
            {
                //se ejecta una vez para crear los roles
                if (!_rolManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                {
                    await _rolManager.CreateAsync(new IdentityRole("admin"));
                    await _rolManager.CreateAsync(new IdentityRole("registrado"));

                }
                //usuario administrador  admin@admin.com Admin123*

                //await _userManager.AddToRoleAsync(usuario, "admin");
                await _userManager.AddToRoleAsync(usuario, "registrado");

                var usuarioReturn = _db.AppUsuario.FirstOrDefault(u => u.UserName == usuarioRegistroDto.NombreUSuario);

                //opcion 1

                //return new UsuarioDatosDto()
                //{
                //    Id = usuarioReturn.Id,
                //    UserName = usuarioReturn.UserName,
                //    Nombre = usuarioReturn.Nombre
                //};

                //opcion2
                return _mapper.Map<UsuarioDatosDto>(usuarioReturn);
            }

            return new UsuarioDatosDto();

        }


        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {

            var usuario = _db.AppUsuario.FirstOrDefault(
                    u => u.UserName.ToLower() == usuarioLoginDto.NombreUSuario.ToLower());

            bool isValida = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

            //valida si el usuario no existe o claves incorrectas
            if (usuario == null || isValida == false)
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token= "",
                    Usuario = null
                };
            }
            
            var roles = await  _userManager.GetRolesAsync(usuario);

            var manejadorToken = new JwtSecurityTokenHandler();

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));

            var tokenDescriptor = new  Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                    
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                //SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)

            };

            var token = manejadorToken.CreateToken(tokenDescriptor);
            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
            };

            return usuarioLoginRespuestaDto;

        }

        public bool BorrarUsuario(AppUsuario usuario)
        {
            _db.AppUsuario.Remove(usuario);
            return Guardar();
        }

        public bool Guardar()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }

        public bool ExisteUsuario(string id)
        {
            bool valor = _db.AppUsuario.Any(c => c.Id == id);
            return valor;
        }
    }
}
