using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

[ApiController]
[ApiExplorerSettings(GroupName = "TodoList")]
public class GetTodoListController(IMediator mediator) : ControllerBase
{
    [HttpGet("/api/todos/list/{todoListId:int}")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    [ProducesResponseType(((int)HttpStatusCode.NotFound))]
    public async Task<IActionResult> Get(int todoListId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetTodoList.Query { TodoListId = todoListId }, cancellationToken);
        return response.TodoList switch
        {
            null => NotFound(),
            _ => Ok()
        };
    }
}

public class GetTodoList
{
    public record Query : IRequest<Response>
    {
        public int TodoListId { get; init; }
    }

    public record Response
    {
        public TodoListDto? TodoList { get; init; }

        public class TodoListDto
        {
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

    public class Handler(TodoContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            Response.TodoListDto? todoList = await context
                .TodoLists
                .Where(tl => tl.TodoListId == request.TodoListId)
                .Select(tl => new Response.TodoListDto
                {
                    Name = tl.Name,
                    Todos = tl.Todos.Select(t => new Response.TodoDto
                    {
                        TodoId = t.TodoId,
                        Name = t.Name,
                        Description = t.Description,
                        IsComplete = t.IsComplete
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new Response
            {
                TodoList = todoList
            };
        }
    }
}
