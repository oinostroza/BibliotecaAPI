namespace MiProyecto.Application.Common.Models;
public class ProcessLabelSettings {
    public string Pendiente { get; set; } = "En espera";
    public string Procesando { get; set; } = "Generando...";
    public string Completado { get; set; } = "Finalizado";
    public string Error { get; set; } = "Falló";
}
