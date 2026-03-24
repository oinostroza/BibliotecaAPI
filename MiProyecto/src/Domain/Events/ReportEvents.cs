namespace MiProyecto.Domain.Events;

// Disparador global
public record GenerateAllReportsRequest();

// Comando para un mes específico
public record GenerateMonthlyReportRequest(int Mes, int Anio);
public record GenerateSinglePdfRequest(int AvisoId);

