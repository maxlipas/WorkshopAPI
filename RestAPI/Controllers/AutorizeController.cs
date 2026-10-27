using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestAPI.Models;
using RestAPI.Models.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutorizeController : ControllerBase
{
    private readonly AuthService service;

    public AutorizeController(AuthService service)
    {
        this.service = service;
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromQuery] string login, [FromQuery] string password)
    {
        var user = await service.Login(login, password);
        if (user is null)
            return BadRequest(new { message = "Неверный логин или пароль" });

        var claims = new List<Claim> { new(ClaimTypes.Name, login) };
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] User user)
    {
        var result = await service.Register(user);
        if (!result.Ok)
            return this.FromResult(result);
        return Ok(user);
    }
}
