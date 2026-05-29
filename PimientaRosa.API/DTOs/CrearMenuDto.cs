namespace PimientaRosa.API.DTOs;

public class CrearMenuDto
{
    public string ImagenUrl { get; set; } = string.Empty;
    public List<string> Entradas { get; set; } = new();
    public List<string> Proteinas { get; set; } = new();
    public List<string> Guarniciones { get; set; } = new();
    public List<string> Bebidas { get; set; } = new();
}