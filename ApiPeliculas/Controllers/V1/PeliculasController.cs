﻿using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace ApiPeliculas.Controllers.V1
{
    [Route("api/v{version:ApiVersion}/peliculas")]
    [ApiController]
    [ApiVersion("1.0")]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;

        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;
        }
        //[AllowAnonymous]
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        ////v1 sin paginacion
        //public IActionResult GetPeliculas()
        //{
        //    var listaPeliculas = _pelRepo.GetPeliculas();

        //    var listaPeliculasDto = new List<PeliculaDto>();

        //    foreach (var lista in listaPeliculas)
        //    {
        //        listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(lista));
        //    }

        //    return Ok(listaPeliculasDto);
        //}

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //v2 con paginacion
        public IActionResult GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 2)
        {
            try
            {
                var totalPeliculas = _pelRepo.GettotalPeliculas();
                var peliculas = _pelRepo.GetPeliculas(pageNumber, pageSize);

                if (peliculas == null || !peliculas.Any())
                {
                    return NotFound("No se encontraron peliculas");
                }

                var peliculasDto = peliculas.Select(p => _mapper.Map<PeliculaDto>(p)).ToList();

                var response = new
                {
                    PageNumber = pageNumber,
                    PageZise = pageSize,
                    TotalPages = (int)Math.Ceiling(totalPeliculas / (double)pageSize),
                    TotalItem = totalPeliculas,
                    Items = peliculas
                };

                return Ok(response);

            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Error al recuperar datos de la aplicacion");
            }
 
        }

        [AllowAnonymous]
        [HttpGet("{peliculaId:Int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetPelicula(int peliculaId)
        {
            var itemPelicula = _pelRepo.GetPelicula(peliculaId);

            if (itemPelicula == null)
                return NotFound();

            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

            return Ok(itemPeliculaDto);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (crearPeliculaDto == null)
                return BadRequest(ModelState);

            if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La pelicula ya existe");
                return StatusCode(404, ModelState);
            }


            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

            //if (!_pelRepo.CrearPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Ocurrio un error al guardar el registro {pelicula.Nombre}");
            //    return StatusCode(500, ModelState);
            //}

            //subida de archivo
            if (crearPeliculaDto.Image != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Image.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\"+ nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    crearPeliculaDto.Image.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;

                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _pelRepo.CrearPelicula(pelicula);

            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]  // con el patch solo actualizo los campos en especifico a diferencia del put que es necesario enviar todos
        [ProducesResponseType(204)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ActualizarPatchPelicula(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid || actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var peliculaExistente = _pelRepo.GetPelicula(peliculaId);

            if (peliculaExistente == null)
            {
                return NotFound($"No se encontró la película con el id {peliculaId}");
            }

            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);

            //if (!_pelRepo.ActualizarPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Ocurrio un error al actualizar el registro {pelicula.Nombre}");
            //    return StatusCode(500, ModelState);
            //}
            //subida de archivo
            if (actualizarPeliculaDto.Image != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Image.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Image.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;

                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }
            _pelRepo.ActualizarPelicula(pelicula);

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [Authorize(Roles = "registrado")]
        [HttpDelete("{peliculaId:int}", Name = "EliminarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public IActionResult EliminarPelicula(int peliculaId)
        {

            if (!_pelRepo.ExistePelicula(peliculaId))
                return NotFound(ModelState);

            var pelicula = _pelRepo.GetPelicula(peliculaId);



            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Ocurrio un error al borrar el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("GetPeliculasCategoria/{categoriaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult GetPeliculasEnCategoria(int categoriaId)
        {
            try
            {
                var listaPelicula = _pelRepo.GetPeliculasEnCategoria(categoriaId);

                if (listaPelicula == null || !listaPelicula.Any())
                    return NotFound($"No se encontraron peliculas en esta categoria {categoriaId}");

                var itemPelicula = listaPelicula.Select(pelicula => _mapper.Map<PeliculaDto>(pelicula)).ToList();

                //foreach (var item in listaPelicula)
                //{
                //    itemPelicula.Add(_mapper.Map<PeliculaDto>(item));
                //}

                return Ok(itemPelicula);
            }
            catch (Exception e) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al recuperar datos");
            }

        }

        [AllowAnonymous]
        [HttpGet("Buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult buscar(string nombre)
        {
            try
            {
                var pelicula = _pelRepo.BuscarPeliculas(nombre.Trim());

                if (!pelicula.Any())
                {
                    return NotFound("No se encontraron peliculas");
                }

                var peliculasDto = _mapper.Map<IEnumerable<PeliculaDto>>(pelicula);

                return Ok(peliculasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error en el servicio");
            }

        }

    }
}
