using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared;
using TodoApi.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger setup for controllers
//builder.Services.AddSwaggerGen(options =>
//{
//    options.TagActionsBy(api =>
//    {
//        if (api.GroupName != null)
//        {
//            return [api.GroupName];
//        }

//        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
//        {
//            return [controllerActionDescriptor.ControllerName];
//        }

//        if (api.ActionDescriptor is ActionDescriptor actionDescriptor)
//        {
//            return [actionDescriptor.DisplayName];
//        }

//        throw new InvalidOperationException("Unable to determine tag for endpoint.");
//    });

//    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".").ToString());
//    options.DocInclusionPredicate((name, api) => true);
//});

builder.Services.AddDbContext<TodoContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

//builder.Services.AddMediatR(cfg =>
//{
//    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
//    cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
//});

//builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapControllers();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseMiddleware<ValidationMiddleware>();

// apply pending migrations for local dev only
if (app.Configuration.GetValue<bool>("LocalMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    await context.Database.MigrateAsync();
}

app.Run();

public partial class Program { }
