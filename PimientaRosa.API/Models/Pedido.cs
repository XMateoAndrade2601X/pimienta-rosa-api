namespace PimientaRosa.API.Models;

public class Pedido
{
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty; // Ej: PR-20260529-001

    // Cliente
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;

    // Selección del menú
    public string Entrada { get; set; } = string.Empty;
    public string Proteina { get; set; } = string.Empty;
    public string Guarnicion { get; set; } = string.Empty;
    public string Bebida { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;

    // Delivery
    public string Universidad { get; set; } = string.Empty;
    public string PabellonPuerta { get; set; } = string.Empty;
    public TimeSpan HoraEntrega { get; set; }

    // Pago
    public decimal Total { get; set; } = 15.00m;
    public string MetodoPago { get; set; } = string.Empty; // Yape, Plin, Tarjeta
    public string EstadoPago { get; set; } = "Pendiente";  // Pendiente, Pagado, Fallido

    // Estado del pedido
    public EstadoPedido Estado { get; set; } = EstadoPedido.Recibido;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public string? NotasAdicionales { get; set; }
}

public enum EstadoPedido
{
    Recibido = 0,
    EnPreparacion = 1,
    EnCamino = 2,
    Entregado = 3,
    Cancelado = 4
}