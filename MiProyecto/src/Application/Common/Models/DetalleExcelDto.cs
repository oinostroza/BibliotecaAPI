namespace MiProyecto.Application.Common.Models;
public record AvisoExcelDto(string Poliza, string Aviso, string Cliente, DateTime Fecha, List<DetalleExcelDto> Cuotas);

public record DetalleExcelDto(string Poliza, string Aviso, int NumeroCuota, decimal TotalCuota);

