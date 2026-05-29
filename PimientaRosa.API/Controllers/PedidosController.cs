using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PimientaRosa.API.Data;
using PimientaRosa.API.DTOs;
using PimientaRosa.API.Hubs;
using PimientaRosa.API.Models;

namespace PimientaRosa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<PedidoHub> _hub;

    public PedidosController(AppDbContext db, IHubContext<PedidoHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    // ✅ POST /api/pedidos — Crear nuevo pedido
    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto dto)
    {
        // Generar número de pedido único: PR-20260529-001
        var hoy = DateTime.Today;
        var pedidosHoy = await _db.Pedidos
            .CountAsync(p => p.FechaCreacion.Date == hoy);

        var numeroPedido = $"PR-{hoy:yyyyMMdd}-{(pedidosHoy + 1):D3}";

        var pedido = new Pedido
        {
            NumeroPedido = numeroPedido,
            NombreCliente = dto.NombreCliente,
            Whatsapp = dto.Whatsapp,
            Entrada = dto.Entrada,
            Proteina = dto.Proteina,
            Guarnicion = dto.Guarnicion,
            Bebida = dto.Bebida,
            Cantidad = dto.Cantidad,
            Total = 15.00m * dto.Cantidad,
            Universidad = dto.Universidad,
            PabellonPuerta = dto.PabellonPuerta,
            HoraEntrega = TimeSpan.Parse(dto.HoraEntrega),
            MetodoPago = dto.MetodoPago,
            Estado = EstadoPedido.Recibido,
            EstadoPago = "Pendiente"
        };

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Total,
            pedido.Estado,
            mensaje = "Pedido creado exitosamente"
        });
    }

    // ✅ GET /api/pedidos/{numeroPedido} — Consultar estado (para el cliente)
    [HttpGet("{numeroPedido}")]
    public async Task<IActionResult> ObtenerPedido(string numeroPedido)
    {
        var pedido = await _db.Pedidos
            .FirstOrDefaultAsync(p => p.NumeroPedido == numeroPedido);

        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        return Ok(new
        {
            pedido.NumeroPedido,
            pedido.NombreCliente,
            pedido.Entrada,
            pedido.Proteina,
            pedido.Guarnicion,
            pedido.Bebida,
            pedido.Cantidad,
            pedido.Total,
            pedido.Universidad,
            pedido.PabellonPuerta,
            pedido.HoraEntrega,
            pedido.MetodoPago,
            pedido.EstadoPago,
            Estado = pedido.Estado.ToString(),
            pedido.FechaCreacion
        });
    }

    // ✅ PATCH /api/pedidos/{id}/estado — Actualizar estado (para ti, el admin)
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoDto dto)
    {
        var pedido = await _db.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        pedido.Estado = dto.NuevoEstado;
        pedido.FechaActualizacion = DateTime.Now;

        await _db.SaveChangesAsync();

        // 🔔 Notificar en tiempo real a quien esté viendo ese pedido
        await _hub.Clients.Group(pedido.NumeroPedido).SendAsync("EstadoActualizado", new
        {
            numeroPedido = pedido.NumeroPedido,
            estado = pedido.Estado.ToString(),
            mensaje = ObtenerMensaje(pedido.Estado),
            fecha = pedido.FechaActualizacion
        });

        return Ok(new
        {
            pedido.NumeroPedido,
            Estado = pedido.Estado.ToString(),
            mensaje = "Estado actualizado"
        });
    }

    // Mensaje amigable según el estado
    private static string ObtenerMensaje(EstadoPedido estado) => estado switch
    {
        EstadoPedido.Recibido => "✅ Recibimos tu pedido",
        EstadoPedido.EnPreparacion => "👨‍🍳 Tu pedido está en preparación",
        EstadoPedido.EnCamino => "🛵 Tu pedido está en camino",
        EstadoPedido.Entregado => "🎉 ¡Tu pedido fue entregado!",
        EstadoPedido.Cancelado => "❌ Tu pedido fue cancelado",
        _ => "Estado actualizado"
    };

    // ✅ GET /api/pedidos — Listar todos (para tu panel admin)
    [HttpGet]
    public async Task<IActionResult> ListarPedidos([FromQuery] string? fecha)
    {
        var query = _db.Pedidos.AsQueryable();

        if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var fechaFiltro))
            query = query.Where(p => p.FechaCreacion.Date == fechaFiltro.Date);
        else
            query = query.Where(p => p.FechaCreacion.Date == DateTime.Today);

        var pedidos = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new {
                p.Id,
                p.NumeroPedido,
                p.NombreCliente,
                p.Whatsapp,
                p.Universidad,
                p.PabellonPuerta,
                p.HoraEntrega,
                p.Total,
                p.MetodoPago,
                p.EstadoPago,
                Estado = p.Estado.ToString(),
                p.FechaCreacion
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    // PATCH /api/pedidos/{id}/confirmar-pago
    [HttpPatch("{id}/confirmar-pago")]
    public async Task<IActionResult> ConfirmarPago(int id)
    {
        var pedido = await _db.Pedidos.FindAsync(id);

        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        if (pedido.EstadoPago == "Pagado")
            return BadRequest(new { mensaje = "Este pedido ya estaba pagado" });

        pedido.EstadoPago = "Pagado";
        pedido.Estado = EstadoPedido.EnPreparacion;
        pedido.FechaActualizacion = DateTime.Now;

        await _db.SaveChangesAsync();

        // 🔔 Notificar al cliente
        await _hub.Clients.Group(pedido.NumeroPedido).SendAsync("EstadoActualizado", new
        {
            numeroPedido = pedido.NumeroPedido,
            estado = pedido.Estado.ToString(),
            mensaje = "👨‍🍳 Pago confirmado, tu pedido está en preparación",
            fecha = pedido.FechaActualizacion
        });

        return Ok(new
        {
            pedido.NumeroPedido,
            pedido.EstadoPago,
            Estado = pedido.Estado.ToString(),
            mensaje = "Pago confirmado, pedido encpreparación"
        });
    }
    // GET /api/pedidos/pendientes-pago
    [HttpGet("pendientes-pago")]
    public async Task<IActionResult> PendientesPago()
    {
        var pedidos = await _db.Pedidos
            .Where(p => p.EstadoPago == "Pendiente ")
            .OrderBy(p => p.FechaCreacion)
            .Select(p => new {
                p.Id,
                p.NumeroPedido,
                p.NombreCliente,
                p.Whatsapp,
                p.Total,
                p.MetodoPago,
                p.Universidad,
                p.HoraEntrega,
                p.FechaCreacion
            })
            .ToListAsync();

        return Ok(pedidos);
    }
}



// DTO para actualizar estado
public class ActualizarEstadoDto
{
    public EstadoPedido NuevoEstado { get; set; }
}