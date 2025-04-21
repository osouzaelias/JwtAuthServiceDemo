namespace JwtAuthServiceDemo.Models;

public class LoginRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}