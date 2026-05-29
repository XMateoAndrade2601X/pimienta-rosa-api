namespace PimientaRosa.API.DTOs;

public class CrearPedidoDto
{
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
    public string HoraEntrega { get; set; } = string.Empty; // "12:00"

    // Pago
    public string MetodoPago { get; set; } = string.Empty;
}