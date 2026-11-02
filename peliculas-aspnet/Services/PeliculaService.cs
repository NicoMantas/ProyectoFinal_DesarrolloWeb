using Microsoft.EntityFrameworkCore;
using PeliculasAspnet.Data;
using PeliculasAspnet.Dtos;
using PeliculasAspnet.Exceptions;
using PeliculasAspnet.Models;

namespace PeliculasAspnet.Services;

public interface IPeliculaService
{
    Task<Pelicula> Create(CreatePeliculaDto dto);

    Task<PeliculaListResponseDto> FindAll(string? nombre, int pagina, int limite);

    Task<Pelicula?> FindOne(int id);

    Task<Pelicula> Update(int id, UpdatePeliculaDto dto);

    Task<Pelicula> Remove(int id);
}

public class PeliculaService : IPeliculaService
{
    private readonly AppDbContext _db;

    public PeliculaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Pelicula> Create(CreatePeliculaDto dto)
    {
        var pelicula = new Pelicula
        {
            Nombre = dto.Nombre ?? string.Empty,
            Sinopsis = dto.Sinopsis ?? string.Empty,
            Imagen = dto.Imagen ?? string.Empty,
        };

        _db.Peliculas.Add(pelicula);
        await _db.SaveChangesAsync();
        return pelicula;
    }

    public async Task<PeliculaListResponseDto> FindAll(string? nombre, int pagina, int limite)
    {
        var skip = (pagina - 1) * limite;
        IQueryable<Pelicula> query = _db.Peliculas;

        if (!string.IsNullOrEmpty(nombre))
        {
            query = query.Where(pelicula => EF.Functions.Like(pelicula.Nombre, $"%{nombre}%"));
        }

        var total = await query.CountAsync();
        var peliculas = await query
            .OrderBy(pelicula => pelicula.Id)
            .Skip(skip)
            .Take(limite)
            .ToListAsync();

        return new PeliculaListResponseDto
        {
            Data = peliculas,
            Meta = new PeliculaMetaDto
            {
                Pagina = pagina,
                Limite = limite,
                Total = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)limite),
            },
        };
    }

    public Task<Pelicula?> FindOne(int id)
    {
        return _db.Peliculas.FirstOrDefaultAsync(pelicula => pelicula.Id == id);
    }

    public async Task<Pelicula> Update(int id, UpdatePeliculaDto dto)
    {
        var pelicula = await _db.Peliculas.FirstOrDefaultAsync(item => item.Id == id);
        if (pelicula is null)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Internal server error", "Internal Server Error");
        }

        if (dto.Nombre is not null)
        {
            pelicula.Nombre = dto.Nombre;
        }

        if (dto.Sinopsis is not null)
        {
            pelicula.Sinopsis = dto.Sinopsis;
        }

        if (dto.Imagen is not null)
        {
            pelicula.Imagen = dto.Imagen;
        }

        await _db.SaveChangesAsync();
        return pelicula;
    }

    public async Task<Pelicula> Remove(int id)
    {
        var pelicula = await _db.Peliculas.FirstOrDefaultAsync(item => item.Id == id);
        if (pelicula is null)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Internal server error", "Internal Server Error");
        }

        _db.Peliculas.Remove(pelicula);
        await _db.SaveChangesAsync();
        return pelicula;
    }
}
