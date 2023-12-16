using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class GetTodoListTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldReturnNullForTodoListThatDoesNotExist()
    {
        int todoListIdThatDoesNotExist = -23464;

        var response = await sliceFixture.SendAsync(new GetTodoList.Query { TodoListId = todoListIdThatDoesNotExist });

        response.TodoList.ShouldBeNull();
    }
}
