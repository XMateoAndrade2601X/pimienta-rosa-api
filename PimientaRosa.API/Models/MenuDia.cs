namespace PimientaRosa.API.Models;

public class MenuDia
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string Entradas { get; set; } = string.Empty;    // JSON
    public string Proteinas { get; set; } = string.Empty;   // JSON
    public string Guarniciones { get; set; } = string.Empty; // JSON
    public string Bebidas { get; set; } = string.Empty;     // JSON
    public bool Activo { get; set; } = true;
}