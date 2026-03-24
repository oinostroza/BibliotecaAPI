using MassTransit;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Entities;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Models;
using Microsoft.EntityFrameworkCore; 

namespace MiProyecto.Infrastructure.Messaging;

public class TodoItemCreatedConsumer : IConsumer<TodoItemCreatedIntegrationEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TodoItemCreatedConsumer> _logger;

    public TodoItemCreatedConsumer(IApplicationDbContext context, ILogger<TodoItemCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

 public async Task Consume(ConsumeContext<TodoItemCreatedIntegrationEvent> context)
    {
        var data = context.Message;
        var existe = await _context.TodoItems.AnyAsync(x => x.Id == data.Id, context.CancellationToken); 

        if (existe) {
            _logger.LogWarning("Mensaje duplicado detectado para ID: {Id}. Ignorando...", data.Id);
            return; 
        }

        _logger.LogInformation("[WORKER-CONSUMER] 📥 Recibido: {Title}", data.Title);

        _context.TodoItems.Add(new TodoItem { ListId = data.ListId, Title = data.Title });
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("[POSTGRES] ✅ Guardado: {Title}", data.Title);
    }
}
