namespace JwtAuthServiceDemo.Models;

public class ApiKeyModel
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}