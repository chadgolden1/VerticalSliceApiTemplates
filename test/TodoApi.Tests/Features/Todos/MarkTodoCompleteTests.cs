using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class MarkTodoCompleteTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldMarkTodoComplete()
    {
        var todoListId = await sliceFixture.SendAsync(new CreateTodoList.Command
        {
            Name = SampleTodoListName(),
            Todos =
            [
                new() { Name = SampleTodoName() }
            ]
        });

        var getTodoListResponse = await sliceFixture.SendAsync(new GetTodoList.Query { TodoListId = todoListId });
        getTodoListResponse.TodoList.ShouldNotBeNull();
        var todo = getTodoListResponse.TodoList.Todos.First();
        todo.IsComplete.ShouldBeFalse();

        await sliceFixture.SendAsync(new MarkTodoComplete.Command { TodoId = todo.TodoId });

        var getTodoListResponse2 = await sliceFixture.SendAsync(new GetTodoList.Query { TodoListId = todoListId });
        getTodoListResponse2.TodoList!.Todos.First().IsComplete.ShouldBeTrue();
    }

    private static string SampleTodoListName() => SampleData.SampleString();
    private static string SampleTodoName() => SampleData.SampleString();
}
