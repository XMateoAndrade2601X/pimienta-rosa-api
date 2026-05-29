using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PimientaRosa.API.Data;
using PimientaRosa.API.DTOs;
using PimientaRosa.API.Models;

namespace PimientaRosa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var admin = await _db.AdminUsers
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (admin == null || !VerificarPassword(dto.Password, admin.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        var token = GenerarToken(admin);

        return Ok(new
        {
            token,
            nombre = admin.Nombre,
            email = admin.Email
        });
    }

    // POST /api/auth/registrar-admin (úsalo UNA sola vez para crear tu usuario)
    [HttpPost("registrar-admin")]
    public async Task<IActionResult> RegistrarAdmin([FromBody] LoginDto dto)
    {
        if (await _db.AdminUsers.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { mensaje = "Ya existe un admin con ese email" });

        var admin = new AdminUser
        {
            Email = dto.Email,
            Nombre = "Administrador",
            PasswordHash = HashPassword(dto.Password)
        };

        _db.AdminUsers.Add(admin);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Admin creado correctamente" });
    }

    // ── Helpers ──────────────────────────────────────────────

    private string GenerarToken(AdminUser admin)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Name, admin.Nombre),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private static bool VerificarPassword(string password, string hash)
        => HashPassword(password) == hash;
}