using FastEndpoints;

namespace TodoApi.Features.Todos;

public class TodoListEndpointGroup : Group
{
    public TodoListEndpointGroup()
    {
        Configure("/api/todos/list", ep =>
        {
            ep.AllowAnonymous();
            ep.Description(routeBuilder =>
            {
                routeBuilder
                    .WithDisplayName("Todo List")
                    .WithGroupName("Todo List")
                    .WithTags("Todo List");
            });
        });
    }
}
