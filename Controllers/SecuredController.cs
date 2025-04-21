using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthServiceDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecuredController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
        return Ok(new { 
            Message = "API segura acessada com sucesso!", 
            ClientId = clientId,
            Roles = roles
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdmin()
    {
        return Ok(new { Message = "Endpoint de administrador acessado com sucesso!" });
    }
}