using Microsoft.EntityFrameworkCore;
using PeliculasAspnet.Data;
using PeliculasAspnet.Models;

namespace PeliculasAspnet.Services;

public interface IUsersService
{
    Task<User> Create(string nombre, string email, string password);

    Task<User?> FindOneByEmail(string email);

    Task<User?> FindOne(int id);
}

public class UsersService : IUsersService
{
    private readonly AppDbContext _db;

    public UsersService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> Create(string nombre, string email, string password)
    {
        var user = new User
        {
            Nombre = nombre,
            Email = email.Trim().ToLowerInvariant(),
            Password = password,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public Task<User?> FindOneByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);
    }

    public Task<User?> FindOne(int id)
    {
        return _db.Users.FirstOrDefaultAsync(user => user.Id == id);
    }
}
