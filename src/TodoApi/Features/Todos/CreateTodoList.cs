using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Shared.Data;
using TodoApi.Shared.Data.Models;

namespace TodoApi.Features.Todos;

[ApiController]
[ApiExplorerSettings(GroupName = "TodoList")]
public class CreateTodoListController(IMediator mediator) : ControllerBase
{
    [HttpPost("api/todos/list")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    [ProducesResponseType(((int)HttpStatusCode.BadRequest))]
    public async Task<IActionResult> Create([FromBody] CreateTodoList.Command request)
    {
        await mediator.Send(request);
        return Ok();
    }
}

public class CreateTodoList
{
    public record Command : IRequest<int>
    {
        public string Name { get; init; } = string.Empty;
        public List<CreateTodoDto> Todos { get; init; } = [];

        public class CreateTodoDto
        {
            public string Name { get; init; } = string.Empty;
            public string? Description { get; init; }
        }
    }

    public class Validator : AbstractValidator<Command>
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

    public class Handler(TodoContext context) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken cancellationToken)
        {
            var newTodoList = new TodoList
            {
                Name = request.Name
            };
            newTodoList.Todos.AddRange(request.Todos.Select(t => new Todo { Name = t.Name, Description = t.Description }).ToList());

            await context.TodoLists.AddAsync(newTodoList, CancellationToken.None);

            await context.SaveChangesAsync(CancellationToken.None);

            return newTodoList.TodoListId;
        }
    }
}
