namespace MiProyecto.Application.Avisos.Queries.GetPeriodoStatus;

public record PeriodoStatusVm(
    int Mes,
    int Anio,
    string Estado,
    int TotalSolicitado,
    int ProcesadosOk,
    int ProcesadosError,
    double PorcentajeAvance,
    double PdfsPorSegundo,
    TimeSpan TiempoTranscurrido
);
