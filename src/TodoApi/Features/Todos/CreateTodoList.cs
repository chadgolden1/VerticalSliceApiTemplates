using FastEndpoints;
using FluentValidation;
using TodoApi.Shared.Data;
using TodoApi.Shared.Data.Models;

namespace TodoApi.Features.Todos;

public static class CreateTodoList
{
    public class Endpoint : Endpoint<Command, Response>
    {
        public override void Configure()
        {
            Post("/");
            Group<TodoListEndpointGroup>();
            Description(builder =>
            {
                builder
                    .WithSummary("Create a new todo list")
                    .WithDescription("Creates a new todo list with optional initial todos")
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
            var newTodoList = new TodoList
            {
                Name = command.Name
            };
            newTodoList.Todos.AddRange(command.Todos.Select(t => new Todo { Name = t.Name, Description = t.Description }).ToList());

            await context.TodoLists.AddAsync(newTodoList, ct);
            await context.SaveChangesAsync(ct);

            return new Response { TodoListId = newTodoList.TodoListId };
        }
    }

    public class Command : ICommand<Response>
    {
        public string Name { get; init; } = string.Empty;
        public List<CreateTodoDto> Todos { get; init; } = [];
    }

    public class Response
    {
        public int TodoListId { get; init; }
    }

    public class CreateTodoDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }

    public class Validator : Validator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleForEach(x => x.Todos).ChildRules(todo =>
            {
                todo.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                todo.RuleFor(x => x.Description).MaximumLength(8000);
            });
        }
    }
}
