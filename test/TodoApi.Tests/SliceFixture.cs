using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Shared.Data;

namespace TodoApi.Tests;

[CollectionDefinition(nameof(SliceFixture))]
public class SliceCollectionFixture : ICollectionFixture<SliceFixture> { }

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Handled in IAsyncLifetime")]
public class SliceFixture : IAsyncLifetime
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public SliceFixture()
    {
        _factory = new TestApplicationFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    private sealed class TestApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:Default", "Server=(localdb)\\MSSQLLocalDB;Database=TodoApiTemplateIntegrationTests;Trusted_Connection=True;TrustServerCertificate=True;" },
                    { "LocalMigrations", "false" }
                }));
        }
    }

    public HttpClient Client => _factory.CreateClient();

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public Task ExecuteDbContextAsync(Func<TodoContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TodoContext>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<TodoContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TodoContext>()));

    public Task InsertAsync<T>(params T[] entities) where T : class =>
        ExecuteDbContextAsync(db =>
        {
            foreach (T entity in entities)
            {
                _ = db.Set<T>().Add(entity);
            }
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class =>
        ExecuteDbContextAsync(db =>
        {
            _ = db.Set<TEntity>().Add(entity);
            return db.SaveChangesAsync();
        });

    public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class =>
            ExecuteDbContextAsync(db =>
            {
                _ = db.Set<TEntity>().Add(entity);
                _ = db.Set<TEntity2>().Add(entity2);
                return db.SaveChangesAsync();
            });

    public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class =>
            ExecuteDbContextAsync(db =>
            {
                _ = db.Set<TEntity>().Add(entity);
                _ = db.Set<TEntity2>().Add(entity2);
                _ = db.Set<TEntity3>().Add(entity3);

                return db.SaveChangesAsync();
            });

    public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class =>
            ExecuteDbContextAsync(db =>
            {
                _ = db.Set<TEntity>().Add(entity);
                _ = db.Set<TEntity2>().Add(entity2);
                _ = db.Set<TEntity3>().Add(entity3);
                _ = db.Set<TEntity4>().Add(entity4);

                return db.SaveChangesAsync();
            });

    public Task<T?> FindAsync<T>(int id)
        where T : class
        => ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) =>
        ExecuteScopeAsync(sp =>
        {
            IMediator mediator = sp.GetRequiredService<IMediator>();
            return mediator.Send(request);
        });

    public Task SendAsync(IRequest request) =>
        ExecuteScopeAsync(sp =>
        {
            IMediator mediator = sp.GetRequiredService<IMediator>();
            return mediator.Send(request);
        });

    public async Task InitializeAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}
