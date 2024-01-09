using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db; //instancio 

        public UsuarioRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }
        public Usuario GetUsuario(int usuarioId)
        {
            throw new NotImplementedException();
        }

        public ICollection<Usuario> GetUsuarios()
        {
            throw new NotImplementedException();
        }

        public bool IsUniqueUser(string usuario)
        {
            throw new NotImplementedException();
        }

        public Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {
            throw new NotImplementedException();
        }
    }
}
