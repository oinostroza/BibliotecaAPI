using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using MiProyecto.Application.Avisos.Commands.BulkCreate;
using MiProyecto.Application.Avisos.Commands.CreateAviso;
using MiProyecto.Domain.Events;

namespace MiProyecto.Web.Endpoints;

public class Avisos : IEndpointGroup
{

   public static void Map(RouteGroupBuilder groupBuilder)
    {
       //groupBuilder.RequireAuthorization();
        groupBuilder.MapPost(CreateAviso);
         // Nuevo endpoint para carga masiva
        groupBuilder.MapPost("/bulk-seed", BulkSeedAvisos)
            .WithName("BulkSeedAvisos")
            .WithDescription("Genera 3000 avisos dummy distribuidos por meses.");

        groupBuilder.MapPost("/generate-reports", GenerateReports);

  
    }
    public static async Task<Accepted> GenerateReports(IBus bus) // Cambia IPublishEndpoint por IBus
    {
        await bus.Publish(new GenerateAllReportsRequest()); // Sale directo a RabbitMQ
        return TypedResults.Accepted("");
    }

    public static async Task<Created<int>> CreateAviso(ISender sender, CreateAvisoCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Avisos)}/{id}", id);
    }
    public static async Task<Ok<string>> BulkSeedAvisos(ISender sender)
    {
        var result = await sender.Send(new BulkCreateAvisosCommand());
        return TypedResults.Ok(result);
    }

}
