using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Constants;
using MiProyecto.Infrastructure.Data;
using MiProyecto.Infrastructure.Data.Interceptors;
using MiProyecto.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MassTransit;
using MiProyecto.Infrastructure.Messaging;
using MiProyecto.Infrastructure.Services;
using MiProyecto.Infrastructure.Messaging.Consumers.Reports;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            //options.UseSqlite(connectionString);
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        builder.Services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = false;
            options.StartTimeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.AddTransient<IExcelService, ExcelService>();
        builder.Services.AddSingleton<IPdfService, PdfService>();

        builder.Services.AddMassTransit(x =>
        {     
            x.AddConsumer<TodoItemCreatedConsumer>();
            x.AddConsumer<AvisoCreatedConsumer>();
            x.AddConsumer<GenerateAllReportsConsumer>();
            x.AddConsumer<GenerateMonthlyReportConsumer>();
            x.AddConsumer<GenerateSinglePdfConsumer>();

            
            // Ahora sí debería reconocerlo:
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres(); 
                o.UseBusOutbox(); 
                o.QueryDelay = TimeSpan.FromSeconds(1);    
                o.DisableInboxCleanupService(); 
                
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ReceiveEndpoint("generate-pdf-queue", e => {
                    e.ConcurrentMessageLimit = 1; 
                    e.ConfigureConsumer<GenerateSinglePdfConsumer>(context);
                });
                cfg.Host("localhost", "/");
                // Configura automáticamente las colas basadas en tus Consumers registrados
                cfg.ConfigureEndpoints(context); 
            });

        });



    }
}
