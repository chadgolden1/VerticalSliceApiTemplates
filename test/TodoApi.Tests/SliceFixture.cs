using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Shared.Data;

namespace TodoApi.Tests;

[CollectionDefinition(nameof(SliceFixture))]
public class SliceCollectionFixture : ICollectionFixture<SliceFixture> { }

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Handled in IAsyncLifetime")]
public class SliceFixture : IAsyncLifetime
{
    public DistributedApplication AppHost { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var testAppBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.TodoApi_AppHost>();

        testAppBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var db = testAppBuilder.Resources.Where(r => r.Name == "todo-sql-server").FirstOrDefault();

        if (db != null)
        {
            var containerLifetimeAnnotation = db.Annotations
                .OfType<ContainerLifetimeAnnotation>()
                .FirstOrDefault();

            if (containerLifetimeAnnotation != null)
            {
                db.Annotations.Remove(containerLifetimeAnnotation);
            }

            var dataVolumeAnnotation = db.Annotations
                .OfType<ContainerMountAnnotation>()
                .FirstOrDefault();

            if (dataVolumeAnnotation != null)
            {
                db.Annotations.Remove(dataVolumeAnnotation);
            }
        }

        AppHost = await testAppBuilder.BuildAsync();

        await AppHost.StartAsync();
    }

    public HttpClient Client => AppHost.CreateHttpClient("todo-api");

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using IServiceScope scope = AppHost.Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using IServiceScope scope = AppHost.Services.CreateScope();
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

    public async Task DisposeAsync()
    {
        if (AppHost != null)
        {
            if (AppHost is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                AppHost.Dispose();
            }
        }
    }
}
