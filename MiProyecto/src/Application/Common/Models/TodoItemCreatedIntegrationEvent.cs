namespace MiProyecto.Application.Common.Models;

// Solo los datos mínimos necesarios
public record TodoItemCreatedIntegrationEvent{  
    public TodoItemCreatedIntegrationEvent(int Id, string Title, int ListId){}
    public TodoItemCreatedIntegrationEvent(){}
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public int ListId { get; init; }
}