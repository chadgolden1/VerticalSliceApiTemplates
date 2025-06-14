using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

public static class ListTodoLists
{
    public class Endpoint : EndpointWithoutRequest<ListTodoLists.Response>
    {
        public override void Configure()
        {
            Get("list");
            AllowAnonymous();
            Group<TodoEndpointGroup>();
            Summary(s =>
            {
                s.Summary = "List all todo lists";
                s.Description = "Returns all todo lists with their todos";
                s.Responses[200] = "Todo lists retrieved successfully";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = await new ListTodoLists.Command().ExecuteAsync(ct);
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
                        Name = tl.Name,
                        Todos = tl.Todos.Select(t => new TodoDto
                        {
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
        public string Name { get; init; } = string.Empty;
        public List<TodoDto> Todos { get; init; } = [];
    }

    public class TodoDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
