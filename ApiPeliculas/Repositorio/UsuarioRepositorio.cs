using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.IdentityModel.Tokens;
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

        public UsuarioRepositorio(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:token");

        }
        public Usuario GetUsuario(int usuarioId)
        {
            return _db.Usuario.FirstOrDefault(u => u.Id == usuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            return _db.Usuario.OrderBy(u => u.NombreUsuario).ToList(); 
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuariodb = _db.Usuario.FirstOrDefault(u => u.NombreUsuario == usuario);

            if (usuariodb == null)
            {
                return true;
            }

            return false;
            
        }

        public  async Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {
            var passwordEncriptado = encriptarMd5(usuarioRegistroDto.Password);

            Usuario usuario = new Usuario()
            {
                NombreUsuario = usuarioRegistroDto.NombreUSuario,
                Password = passwordEncriptado,
                Nombre = usuarioRegistroDto.Nombre,
                Role = usuarioRegistroDto.Role
            };

            _db.Usuario.Add(usuario);
            await _db.SaveChangesAsync();

            usuario.Password = passwordEncriptado;

            return usuario;

        }


        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {

            var passwordEncriptado = encriptarMd5(usuarioLoginDto.Password);

            var usuario = _db.Usuario.FirstOrDefault(
                    u => u.NombreUsuario.ToLower() == usuarioLoginDto.NombreUSuario.ToLower()
                            &&
                         u.Password == passwordEncriptado
                   );

            if (usuario == null)
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token= "",
                    Usuario = null
                };
            }

            var manejadorToken = new JwtSecurityTokenHandler();

            //var key = Encoding.ASCII.GetBytes(claveSecreta);
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));

            var tokenDescriptor = new  Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario.ToString()),
                    new Claim(ClaimTypes.Role, usuario.Role)
                    
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                //SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)

            };

            var token = manejadorToken.CreateToken(tokenDescriptor);
            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = usuario
            };

            return usuarioLoginRespuestaDto;

        }

        public static string encriptarMd5(string valor)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
            data = x.ComputeHash(data);
            string resp = "";
            for (int i = 0; i < data.Length; i++)
                resp += data[i].ToString("x2").ToLower();
            return resp;
        }

        public bool BorrarUsuario(Usuario usuario)
        {
            _db.Usuario.Remove(usuario);
            return Guardar();
        }

        public bool Guardar()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }

        public bool ExisteUsuario(int id)
        {
            bool valor = _db.Usuario.Any(c => c.Id == id);
            return valor;
        }
    }
}
