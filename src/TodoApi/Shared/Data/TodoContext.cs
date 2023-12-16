using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data.Models;

namespace TodoApi.Shared.Data;

public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var todoList = modelBuilder.Entity<TodoList>();
        todoList.Property(x => x.Name).HasMaxLength(100);
        todoList.HasMany(x => x.Todos).WithOne(x => x.TodoList);

        var todo = modelBuilder.Entity<Todo>();
        todo.Property(x => x.Name).HasMaxLength(100);
        todo.Property(x => x.Description).HasMaxLength(8000);
    }
}
