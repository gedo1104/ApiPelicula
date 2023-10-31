using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IPeliculaRepositorio
    {
        ICollection<Pelicula> GetPeliculas();
        Pelicula GetPelicula(int peliculaId);
        bool ExistePelicula(string nombre);
        bool ExistePelicula(int id);
        bool CrearPelicula(Pelicula pelicula);
        bool ActualizarPelicula(Pelicula pelicula);
        bool BorrarPelicula(Pelicula pelicula);

        //buscar peli en categoria
        ICollection<Pelicula> GetPeliculasEnCategoria(int catId);

        //buscar peli por nombre
        ICollection<Pelicula> BuscarPeliculas(string nombre);


        bool Guardar();


    }
}
