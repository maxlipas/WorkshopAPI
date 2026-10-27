using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services;

public class AuthService
{
    private readonly AppDbContext db;

    public AuthService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<User?> Login(string login, string password) =>
        await db.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password);

    public async Task<Result> Register(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login) || string.IsNullOrWhiteSpace(user.Password))
            return Result.Fail("Логин и пароль обязательны");
        if (await db.Users.AnyAsync(u => u.Login == user.Login))
            return Result.Conflict("Пользователь с таким логином уже есть");

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Result.Success();
    }
}
