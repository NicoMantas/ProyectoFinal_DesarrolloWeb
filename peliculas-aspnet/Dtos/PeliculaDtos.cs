namespace PeliculasAspnet.Dtos;

public class CreatePeliculaDto
{
    public string? Nombre { get; set; }

    public string? Sinopsis { get; set; }

    public string? Imagen { get; set; }
}

public class UpdatePeliculaDto
{
    public string? Nombre { get; set; }

    public string? Sinopsis { get; set; }

    public string? Imagen { get; set; }
}

public class PeliculaMetaDto
{
    public int Pagina { get; set; }

    public int Limite { get; set; }

    public int Total { get; set; }

    public int TotalPaginas { get; set; }
}

public class PeliculaListResponseDto
{
    public List<Models.Pelicula> Data { get; set; } = [];

    public PeliculaMetaDto Meta { get; set; } = new();
}
