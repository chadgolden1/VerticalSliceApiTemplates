using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.AddSqlServerDbContext<TodoContext>("todo-db");

builder.Services
    .AddFastEndpoints(o =>
    {
        o.IncludeAbstractValidators = true;
    })
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Todo API";
            s.Version = "v1";
        };
        o.AutoTagPathSegmentIndex = 0;
    });

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapControllers();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

// Apply migrations when running in Aspire (detected by OTEL_SERVICE_NAME) or when LocalMigrations is true
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")) ||
    app.Configuration.GetValue<bool>("LocalMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    await context.Database.MigrateAsync();
}

app.Run();

public partial class Program { }
