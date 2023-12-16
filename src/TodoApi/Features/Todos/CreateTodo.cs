using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;
using TodoApi.Shared.Data.Models;

namespace TodoApi.Features.Todos;

[ApiController]
[ApiExplorerSettings(GroupName = "Todo")]
public class CreateTodoController(IMediator mediator) : ControllerBase
{
    [HttpPost("api/todos/todo")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    [ProducesResponseType(((int)HttpStatusCode.BadRequest))]
    public async Task<IActionResult> Create([FromBody] CreateTodo.Command request)
    {
        await mediator.Send(request);
        return Ok();
    }
}

public class CreateTodo
{
    public record Command : IRequest<int>
    {
        public int TodoListId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(TodoContext context)
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(8000);
            RuleFor(x => x.TodoListId).MustAsync(async (id, cancellationToken) =>
            {
                return await context.TodoLists.AnyAsync(x => x.TodoListId == id, cancellationToken);
            }).WithMessage("Todo list does not exist.");
        }
    }

    public class Handler(TodoContext context) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken cancellationToken)
        {
            var todoList = await context.TodoLists.FirstAsync(x => x.TodoListId == request.TodoListId, cancellationToken);

            var todo = new Todo
            {
                Name = request.Name,
                Description = request.Description,
                TodoList = todoList
            };

            await context.Todos.AddAsync(todo, CancellationToken.None);

            await context.SaveChangesAsync(CancellationToken.None);

            return todo.TodoId;
        }
    }
}
