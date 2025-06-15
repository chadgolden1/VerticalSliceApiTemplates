using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TodoApi.Shared.Data;

namespace TodoApi.Features.Todos;

public static class MarkTodoComplete
{
    public class Endpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Put("/{todoId:int}/mark-complete");
            Group<TodoEndpointGroup>();
            Description(builder =>
            {
                builder
                    .WithSummary("Mark todo as complete")
                    .WithDescription("Marks a specific todo item as completed")
                    .Produces(404);
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var command = new Command
            {
                TodoId = Route<int>("todoId")
            };

            var response = await command.ExecuteAsync(ct);

            if (!response.Exists)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
    }

    public class Handler(TodoContext context) : CommandHandler<Command, Response>
    {
        public override async Task<Response> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var todo = await context.Todos.FirstOrDefaultAsync(x => x.TodoId == command.TodoId, ct);

            if (todo is null)
            {
                return new Response(false);
            }

            todo.IsComplete = true;

            await context.SaveChangesAsync(ct);

            return new Response(true);
        }
    }

    public class Command : ICommand<Response>
    {
        public int TodoId { get; init; }
    }

    public record Response(bool Exists);
}
