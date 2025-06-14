using FastEndpoints;

namespace TodoApi.Features.Todos;

public class TodoEndpointGroup : Group
{
    public TodoEndpointGroup()
    {
        Configure("/api/todos", ep =>
        {
            ep.Description(routeBuilder =>
            {
                routeBuilder
                    .WithDisplayName("Todo")
                    .WithName("Todo");
            });
        });
    }
}
