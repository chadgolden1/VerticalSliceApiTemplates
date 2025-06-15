using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

public static class ListTodoLists
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public override void Configure()
        {
            Get("/");
            Group<TodoListEndpointGroup>();
            Description(builder =>
            {
                builder
                    .WithSummary("List all todo lists")
                    .WithDescription("Returns all todo lists with their todos");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = await new Command().ExecuteAsync(ct);
            await SendOkAsync(response, ct);
        }
    }

    public class Handler(TodoContext context) : CommandHandler<Command, Response>
    {
        public override async Task<Response> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var response = new Response
            {
                TodoLists = await context.TodoLists
                    .OrderBy(tl => tl.TodoListId)
                    .Select(tl => new TodoListDto
                    {
                        TodoListId = tl.TodoListId,
                        Name = tl.Name,
                        Todos = tl.Todos.Select(t => new TodoDto
                        {
                            TodoId = t.TodoId,
                            Name = t.Name,
                            Description = t.Description
                        }).ToList()
                    })
                    .ToListAsync(ct)
            };

            return response;
        }
    }

    public class Command : ICommand<Response> { }

    public class Response
    {
        public List<TodoListDto> TodoLists { get; init; } = [];
    }

    public class TodoListDto
    {
        public int TodoListId { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<TodoDto> Todos { get; init; } = [];
    }

    public class TodoDto
    {
        public int TodoId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
