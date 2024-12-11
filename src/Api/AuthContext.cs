using Microsoft.EntityFrameworkCore;

namespace Api;

public class AuthContext : DbContext
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}

public static class AuthContextExntexsions
{
    public static void Init(this AuthContext _dbContext)
    {
        _dbContext.Database.EnsureCreated();

        if(!_dbContext.Users.Any())
        {
            _dbContext.Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@example.com",
                Password = "P@ssw0rd",
            });
            _dbContext.SaveChanges();
        }
    }
}
