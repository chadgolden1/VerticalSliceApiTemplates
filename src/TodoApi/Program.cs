using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<TodoContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

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

app.UseHttpsRedirection();

app.MapControllers();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

// apply pending migrations for local dev only
if (app.Configuration.GetValue<bool>("LocalMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    await context.Database.MigrateAsync();
}

app.Run();

public partial class Program { }
