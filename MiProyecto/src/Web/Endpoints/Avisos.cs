using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using MiProyecto.Application.Avisos.Commands.BulkCreate;
using MiProyecto.Application.Avisos.Commands.CreateAviso;
using MiProyecto.Application.Avisos.Commands.RetryFailedPdfs;
using MiProyecto.Application.Avisos.Queries.GetPeriodoStatus;
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

        
        groupBuilder.MapPost("/retry-failed", RetryFailed);
        groupBuilder.MapPost("/get-status", GetStatus);
        groupBuilder.MapGet("/periodo-status/{anio}/{mes}", GetPeriodoStatus);

        groupBuilder.MapPost("/generar-periodo/{anio}/{mes}", GenerarPeriodo)
            .WithName("GenerarPeriodoPdf")
            .WithDescription("Inicia la generación masiva de PDFs para un mes y año específicos");


  
    }
    
    public static async Task<IResult> GenerarPeriodo(int anio, int mes, ISender sender)
    {
        try 
        {
            // Enviamos el comando al Handler que creamos con la lógica de periodo
            var totalEncolados = await sender.Send(new GeneratePeriodPdfCommand(mes, anio));
            
            if (totalEncolados == 0)
            {
                return TypedResults.BadRequest(new { 
                    Mensaje = $"No se encontraron avisos pendientes para el periodo {mes}/{anio}." 
                });
            }

            return TypedResults.Ok(new { 
                Mensaje = $"🚀 Proceso iniciado exitosamente.",
                TotalAvisos = totalEncolados,
                Periodo = $"{mes}/{anio}"
            });
        }
        catch (Exception ex)
        {
            return TypedResults.Conflict(new { Error = ex.Message });
        }
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
    public static async Task<IResult> GetPeriodoStatus(int anio, int mes, ISender sender)
    {
        var result = await sender.Send(new GetPeriodoStatusQuery(mes, anio));
        
        return result is not null 
            ? TypedResults.Ok(result) 
            : TypedResults.NotFound(new { Mensaje = "No hay procesos para ese periodo." });
    }

    public static async Task<IResult> RetryFailed(ISender sender)
    {
        var count = await sender.Send(new RetryFailedPdfsCommand());
        return TypedResults.Ok(new { Mensaje = $"Se han re-encolado {count} procesos fallidos." });
    }

}
