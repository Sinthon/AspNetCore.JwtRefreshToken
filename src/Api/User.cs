namespace Api;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresOn { get; set; }

    public User User { get; set; }
}
