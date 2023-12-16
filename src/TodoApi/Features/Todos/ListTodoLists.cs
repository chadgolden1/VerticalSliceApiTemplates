using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

[ApiController]
[ApiExplorerSettings(GroupName = "TodoList")]
public class ListTodoListsController(IMediator mediator) : ControllerBase
{
    [HttpGet("/api/todos/list")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ListTodoLists.Query(), cancellationToken));
}

public static class ListTodoLists
{
    public record Query : IRequest<Response> { }

    public record Response
    {
        public List<TodoListDto> TodoLists { get; init; } = [];

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

    public class Handler(TodoContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            return new Response
            {
                TodoLists = await context.TodoLists
                    .OrderBy(tl => tl.TodoListId)
                    .Select(tl => new Response.TodoListDto
                    {
                        Name = tl.Name,
                        Todos = tl.Todos.Select(t => new Response.TodoDto
                        {
                            Name = t.Name,
                            Description = t.Description
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken)
            };
        }
    }
}
