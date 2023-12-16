using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

[ApiController]
[ApiExplorerSettings(GroupName = "Todo")]
public class MarkTodoCompleteController(IMediator mediator) : ControllerBase
{
    [HttpPut("/api/todos/todo/{todoId:int}/mark-complete")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    [ProducesResponseType(((int)HttpStatusCode.BadRequest))]
    public async Task<IActionResult> MarkTodoComplete(int todoId)
    {
        await mediator.Send(new MarkTodoComplete.Command { TodoId = todoId });
        return Ok();
    }
}

public class MarkTodoComplete
{
    public record Command : IRequest
    {
        public int TodoId { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(TodoContext context)
        {
            RuleFor(x => x.TodoId).MustAsync(async (todoId, cancellationToken) =>
            {
                return await context.Todos.AnyAsync(x => x.TodoId == todoId, cancellationToken);
            }).WithMessage("Todo could not found");
        }
    }

    public class Handler(TodoContext context) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var todo = await context.Todos.FirstAsync(x => x.TodoId == request.TodoId, cancellationToken);

            todo.IsComplete = true;

            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
