using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasAspnet.Dtos;
using PeliculasAspnet.Services;

namespace PeliculasAspnet.Controllers;

[ApiController]
[Route("pelicula")]
public class PeliculaController : ControllerBase
{
    private readonly IPeliculaService _peliculas;

    public PeliculaController(IPeliculaService peliculas)
    {
        _peliculas = peliculas;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePeliculaDto dto)
    {
        var created = await _peliculas.Create(dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet]
    public Task<PeliculaListResponseDto> FindAll(
        [FromQuery] string? nombre,
        [FromQuery] string? pagina,
        [FromQuery] string? limite)
    {
        return _peliculas.FindAll(
            nombre,
            QueryNumbers.OrDefault(pagina, 1),
            QueryNumbers.OrDefault(limite, 5));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> FindOne(int id)
    {
        var pelicula = await _peliculas.FindOne(id);
        if (pelicula is null)
        {
            return Content("null", "application/json");
        }

        return Ok(pelicula);
    }

    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePeliculaDto dto)
    {
        var updated = await _peliculas.Update(id, dto);
        return Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var removed = await _peliculas.Remove(id);
        return Ok(removed);
    }
}
