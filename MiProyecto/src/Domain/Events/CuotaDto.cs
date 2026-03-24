namespace MiProyecto.Domain.Events; // Ajusta el namespace a tu carpeta de eventos

public class CuotaDto
{
    public int NumeroCuota { get; set; }
    public decimal TotalCuota { get; set; }
    public string? Observacion { get; set; } // Opcional, por si quieres guardar un texto por cuota
}
