using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using TodoApi.Shared.Data;

namespace TodoApi.MigrationService;

public class TodoDbInitializer(
    IHostEnvironment hostEnvironment,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<TodoDbInitializer> logger) : BackgroundService
{
    private readonly ActivitySource _activitySource = new(hostEnvironment.ApplicationName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity(hostEnvironment.ApplicationName, ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();

            logger.LogInformation("Starting database migration...");

            await dbContext.Database.MigrateAsync(stoppingToken);

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}
