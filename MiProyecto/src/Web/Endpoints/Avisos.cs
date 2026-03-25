using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using MiProyecto.Application.Avisos.Commands.BulkCreate;
using MiProyecto.Application.Avisos.Commands.CreateAviso;
using MiProyecto.Application.Avisos.Commands.RetryFailedPdfs;
using MiProyecto.Application.Avisos.Queries.GetProcesosStatus;
using MiProyecto.Domain.Events;

namespace MiProyecto.Web.Endpoints;

public class Avisos : IEndpointGroup
{

   public static void Map(RouteGroupBuilder groupBuilder)
    {
       //groupBuilder.RequireAuthorization();
        groupBuilder.MapPost(CreateAviso);
        groupBuilder.MapPost("/bulk-seed", BulkSeedAvisos)
            .WithName("BulkSeedAvisos")
            .WithDescription("Genera 3000 avisos dummy distribuidos por meses.");

        groupBuilder.MapPost("/generate-reports", GenerateReports);
        groupBuilder.MapPost("/retry-failed", RetryFailed);
        groupBuilder.MapPost("/get-status", GetStatus);
  
    }
    public static async Task<Accepted> GenerateReports(IBus bus) 
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
    public static async Task<ProcesosStatusVm> GetStatus(ISender sender)
    {
        return await sender.Send(new GetProcesosStatusQuery());
    }
    public static async Task<IResult> RetryFailed(ISender sender)
    {
        var count = await sender.Send(new RetryFailedPdfsCommand());
        return TypedResults.Ok(new { Mensaje = $"Se han re-encolado {count} procesos fallidos." });
    }

}
