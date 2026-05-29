using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PimientaRosa.API.Data;
using PimientaRosa.API.DTOs;
using PimientaRosa.API.Models;

namespace PimientaRosa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _db;

    public MenuController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/menu/hoy — el frontend lo llama para mostrar el menú del día
    [HttpGet("hoy")]
    public async Task<IActionResult> ObtenerMenuHoy()
    {
        var menu = await _db.MenusDia
            .Where(m => m.Fecha.Date == DateTime.Today && m.Activo)
            .FirstOrDefaultAsync();

        if (menu == null)
            return NotFound(new { mensaje = "No hay menú publicado para hoy" });

        return Ok(new
        {
            menu.Id,
            menu.Fecha,
            menu.ImagenUrl,
            Entradas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(menu.Entradas),
            Proteinas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(menu.Proteinas),
            Guarniciones = System.Text.Json.JsonSerializer.Deserialize<List<string>>(menu.Guarniciones),
            Bebidas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(menu.Bebidas)
        });
    }

    // POST /api/menu — publicar el menú del día (solo admin lo usará)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PublicarMenu([FromBody] CrearMenuDto dto)
    {
        // Desactivar menú anterior si existe
        var menuAnterior = await _db.MenusDia
            .Where(m => m.Fecha.Date == DateTime.Today && m.Activo)
            .FirstOrDefaultAsync();

        if (menuAnterior != null)
            menuAnterior.Activo = false;

        var menu = new MenuDia
        {
            Fecha = DateTime.Today,
            ImagenUrl = dto.ImagenUrl,
            Entradas = System.Text.Json.JsonSerializer.Serialize(dto.Entradas),
            Proteinas = System.Text.Json.JsonSerializer.Serialize(dto.Proteinas),
            Guarniciones = System.Text.Json.JsonSerializer.Serialize(dto.Guarniciones),
            Bebidas = System.Text.Json.JsonSerializer.Serialize(dto.Bebidas),
            Activo = true
        };

        _db.MenusDia.Add(menu);
        await _db.SaveChangesAsync();

        return Ok(new { menu.Id, mensaje = "Menú publicado correctamente" });
    }

    // GET /api/menu — historial de menús (opcional, útil para admin)
    [HttpGet]
    public async Task<IActionResult> Historial()
    {
        var menus = await _db.MenusDia
            .OrderByDescending(m => m.Fecha)
            .Take(30)
            .Select(m => new { m.Id, m.Fecha, m.ImagenUrl, m.Activo })
            .ToListAsync();

        return Ok(menus);
    }

    // DELETE /api/menu/{id} — desactivar un menú
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Desactivar(int id)
    {
        var menu = await _db.MenusDia.FindAsync(id);
        if (menu == null) return NotFound();

        menu.Activo = false;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Menú desactivado" });
    }
}