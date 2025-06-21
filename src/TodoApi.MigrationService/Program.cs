using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoApi.MigrationService;
using TodoApi.Shared.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<TodoContext>("todo-db", configureDbContextOptions: options =>
{
    options.UseSqlServer(sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("TodoApi.MigrationService");
    });
});

builder.Services.AddHostedService<TodoDbInitializer>();

builder.EnrichSqlServerDbContext<TodoContext>();

var app = builder.Build();

app.Run();
