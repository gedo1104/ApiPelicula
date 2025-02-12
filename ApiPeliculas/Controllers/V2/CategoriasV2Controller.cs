using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V2
{
    [ApiController]
    //[Route("Api/[controller]")]  forma predeterminada
    [Route("Api/v{version:ApiVersion}/Categorias")]
    [ApiVersion("2.0")]

    public class CategoriasV2Controller : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;

        private readonly IMapper _mapper;

        public CategoriasV2Controller(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "valor 1", "valor 2", "valor 3" };
        }



    }
}
