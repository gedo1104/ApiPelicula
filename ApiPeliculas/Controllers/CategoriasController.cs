using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    [ApiController]
    //[Route("Api/[controller]")]  forma predeterminada
    [Route("Api/v{version:ApiVersion}/Categorias")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;

        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet]
        [MapToApiVersion("1.0")]
        //[ResponseCache(Duration =20)]
        // [ResponseCache(Location =ResponseCacheLocation.None, NoStore =true)] //no queremos cachear ni guardar los errores en cache
        //[HttpGet("Buscar")]
        [ResponseCache(CacheProfileName = "Default20seconds")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetCategorias()
        {
            var listaCategorias = _ctRepo.GetCategorias();

            var listaCategoriasDto = new List<CategoriaDto>();

            foreach (var lista in listaCategorias)
            {
                listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(lista));
            }

            return Ok(listaCategoriasDto);
        }


        [AllowAnonymous]
        [HttpGet("{categoriaId:Int}", Name= "GetCategoria")]
        //[ResponseCache(Duration = 30)]
        [ResponseCache(CacheProfileName= "Default20seconds")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetCategoria(int categoriaId)
        {
            var itemCategoria = _ctRepo.GetCategoria(categoriaId);

            if (itemCategoria == null)
                return NotFound();

            var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);

            return Ok(itemCategoriaDto);
        }

        [Authorize(Roles ="admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(CategoriaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (crearCategoriaDto == null)
                return BadRequest(ModelState);

            if (_ctRepo.ExisteCategoria(crearCategoriaDto.Nombre))
            {
                ModelState.AddModelError("", "La categoria ya existe");
                return StatusCode(404, ModelState);
            }
                

            var categoria = _mapper.Map<Categoria>(crearCategoriaDto);

            if (!_ctRepo.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"Ocurrio un error al guardar el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetCategoria", new {categoriaId = categoria.Id }, categoria);
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{categoriaId:int}", Name = "ActualizarCategoria")]  // con el patch solo actualizo los campos en especifico a diferencia del put que es necesario enviar todos
        [ProducesResponseType(201, Type = typeof(CategoriaDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public IActionResult ActualizarCategoria(int categoriaId,[FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoriaDto == null || categoriaId != categoriaDto.Id)
                return BadRequest(ModelState);


            var categoria = _mapper.Map<Categoria>(categoriaDto);

            if (!_ctRepo.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Ocurrio un error al actualizar el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{categoriaId:int}", Name = "EliminarCategoria")]  // con el patch solo actualizo los campos en especifico a diferencia del put que es necesario enviar todos
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public IActionResult EliminarCategoria(int categoriaId)
        {

            if (!_ctRepo.ExisteCategoria(categoriaId))
                return NotFound(ModelState);

            var categoria = _ctRepo.GetCategoria(categoriaId);



            if (!_ctRepo.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Ocurrio un error al borrar el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
