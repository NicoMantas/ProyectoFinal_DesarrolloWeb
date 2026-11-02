using Microsoft.EntityFrameworkCore;
using PeliculasAspnet.Models;

namespace PeliculasAspnet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Pelicula> Peliculas => Set<Pelicula>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pelicula>(entity =>
        {
            entity.Property(pelicula => pelicula.Nombre).IsRequired();
            entity.Property(pelicula => pelicula.Sinopsis).IsRequired();
            entity.Property(pelicula => pelicula.Imagen).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Nombre).IsRequired();
            entity.Property(user => user.Email).IsRequired();
            entity.Property(user => user.Password).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });
    }
}
