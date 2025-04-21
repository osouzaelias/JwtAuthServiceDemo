using JwtAuthServiceDemo.Models;
using JwtAuthServiceDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthServiceDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(JwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] LoginRequest request)
    {
        try
        {
            var apiKey = await _jwtService.ValidateClientCredentialsAsync(request.ClientId, request.ClientSecret);
                
            if (apiKey == null)
            {
                return Unauthorized();
            }

            var token = await _jwtService.GenerateTokenAsync(apiKey.ClientId, apiKey.Roles);
            var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                .GetSection("JwtSettings");
                
            var response = new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryInMinutes"))
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, "Internal server error");
        }
    }
}