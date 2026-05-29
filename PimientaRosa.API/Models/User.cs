namespace PimientaRosa.API.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty; // WhatsApp
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}