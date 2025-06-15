using FastEndpoints;

namespace TodoApi.Features.Todos;

public class TodoEndpointGroup : Group
{
    public TodoEndpointGroup()
    {
        Configure("/api/todos", ep =>
        {
            ep.AllowAnonymous();
            ep.Description(routeBuilder =>
            {
                routeBuilder
                    .WithDisplayName("Todo")
                    .WithGroupName("Todo")
                    .WithTags("Todo");
            });
        });
    }
}
