using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Events;
using MassTransit;
using MiProyecto.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace MiProyecto.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }

    public string? Title { get; init; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;

    public CreateTodoItemCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint,
        ILogger<CreateTodoItemCommandHandler> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint; 
        _logger = logger;

    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false
        };

        // Esto es interno del Domain (Memoria)
        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));
             
        await _publishEndpoint.Publish(new TodoItemCreatedIntegrationEvent 
        { 
            Id = entity.Id, 
            Title = entity.Title ?? "Sin Título", // Aseguramos que no sea null
            ListId = entity.ListId 
        }, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken); 
        
        _logger.LogInformation("[WORKER-INSCRIPCION] 📥 Recibido: {Title}", entity.Title);


        return entity.Id;
    }

}
