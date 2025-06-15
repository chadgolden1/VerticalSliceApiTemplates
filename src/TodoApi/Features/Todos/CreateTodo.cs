using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;
using TodoApi.Shared.Data.Models;

namespace TodoApi.Features.Todos;

public static class CreateTodo
{
    public class Endpoint : Endpoint<Command, Response>
    {
        public override void Configure()
        {
            Post("/");
            Group<TodoEndpointGroup>();
            Description(builder =>
            {
                builder
                    .WithSummary("Create a new todo")
                    .WithDescription("Creates a new todo item in an existing todo list")
                    .Produces(400);
            });
        }

        public override async Task HandleAsync(Command command, CancellationToken ct)
        {
            var response = await command.ExecuteAsync(ct);
            await SendOkAsync(response, ct);
        }
    }

    public class Handler(TodoContext context) : CommandHandler<Command, Response>
    {
        public override async Task<Response> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var todoList = await context.TodoLists.FirstAsync(x => x.TodoListId == command.TodoListId, ct);

            var todo = new Todo
            {
                Name = command.Name,
                Description = command.Description,
                TodoList = todoList
            };

            await context.Todos.AddAsync(todo, ct);
            await context.SaveChangesAsync(ct);

            return new Response { TodoId = todo.TodoId };
        }
    }

    public class Command : ICommand<Response>
    {
        public int TodoListId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }

    public class Response
    {
        public int TodoId { get; init; }
    }

    public class Validator : Validator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(8000);
            RuleFor(x => x.TodoListId).MustAsync(async (id, cancellationToken) =>
            {
                return await Resolve<TodoContext>().TodoLists.AnyAsync(x => x.TodoListId == id, cancellationToken);
            }).WithMessage("Todo list does not exist.");
        }
    }
}
