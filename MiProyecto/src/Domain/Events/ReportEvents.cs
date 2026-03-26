namespace MiProyecto.Domain.Events;


public record GenerateMonthlyReportRequest(int Mes, int Anio);
public record GenerateSinglePdfRequest(int AvisoId, int PeriodoId);


