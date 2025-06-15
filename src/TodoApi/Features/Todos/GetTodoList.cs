using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

public static class GetTodoList
{
    public class Endpoint : Endpoint<Command, Response>
    {
        public override void Configure()
        {
            Get("/{todoListId}");
            Group<TodoListEndpointGroup>();
            Description(builder =>
            {
                builder
                    .WithSummary("Get a specific todo list")
                    .WithDescription("Returns a todo list with all its todos by ID")
                    .Produces(404);
            });
        }

        public override async Task HandleAsync(Command command, CancellationToken ct)
        {
            var response = await command.ExecuteAsync(ct);

            if (response.TodoList == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(response, ct);
        }
    }

    public class Handler(TodoContext context) : CommandHandler<Command, Response>
    {
        public override async Task<Response> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            TodoListDto? todoList = await context
                .TodoLists
                .Where(tl => tl.TodoListId == command.TodoListId)
                .Select(tl => new TodoListDto
                {
                    TodoListId = tl.TodoListId,
                    Name = tl.Name,
                    Todos = tl.Todos.Select(t => new TodoDto
                    {
                        TodoId = t.TodoId,
                        Name = t.Name,
                        Description = t.Description,
                        IsComplete = t.IsComplete
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            return new Response
            {
                TodoList = todoList
            };
        }
    }

    public class Command : ICommand<Response>
    {
        [FromRoute]
        public int TodoListId { get; init; }
    }

    public class Response
    {
        public TodoListDto? TodoList { get; init; }
    }

    public class TodoListDto
    {
        public int TodoListId { get; set; }
        public string Name { get; init; } = string.Empty;
        public List<TodoDto> Todos { get; init; } = [];
    }

    public class TodoDto
    {
        public int TodoId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsComplete { get; init; }
    }
}
