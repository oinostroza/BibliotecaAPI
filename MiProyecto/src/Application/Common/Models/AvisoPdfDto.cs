namespace MiProyecto.Application.Common.Models;

public class AvisoPdfDto
{
    public string NumeroPoliza { get; set; } = string.Empty;
    public string NumeroAviso { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string RutCliente { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public List<CuotaPdfDto> Cuotas { get; set; } = new();
    public decimal TotalPagar => Cuotas.Sum(x => x.Monto);
}

public class CuotaPdfDto
{
    public int Numero { get; set; }
    public DateTime Vencimiento { get; set; }
    public decimal Monto { get; set; }
}
