using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class MarkTodoCompleteTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldMarkTodoComplete()
    {
        var createResponse = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = SampleTodoListName(),
            Todos =
            [
                new() { Name = SampleTodoName() }
            ]
        });
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTodoList.Response>();
        var todoListId = createResult!.TodoListId;

        var getResponse = await sliceFixture.Client.GetAsync($"/api/todos/list/{todoListId}");
        getResponse.EnsureSuccessStatusCode();
        var getTodoListResponse = await getResponse.Content.ReadFromJsonAsync<GetTodoList.Response>();
        getTodoListResponse!.TodoList.ShouldNotBeNull();
        var todo = getTodoListResponse.TodoList.Todos.First();
        todo.IsComplete.ShouldBeFalse();

        var markCompleteResponse = await sliceFixture.Client.PutAsync($"/api/todos/{todo.TodoId}/mark-complete", null);
        markCompleteResponse.EnsureSuccessStatusCode();

        var getResponse2 = await sliceFixture.Client.GetAsync($"/api/todos/list/{todoListId}");
        getResponse2.EnsureSuccessStatusCode();
        var getTodoListResponse2 = await getResponse2.Content.ReadFromJsonAsync<GetTodoList.Response>();
        getTodoListResponse2!.TodoList!.Todos.First().IsComplete.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnNotFoundForNonExistentTodo()
    {
        int nonExistentTodoId = -9129394;

        var response = await sliceFixture.Client.PutAsync($"/api/todos/{nonExistentTodoId}/mark-complete", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static string SampleTodoListName() => SampleData.SampleString();
    private static string SampleTodoName() => SampleData.SampleString();
}
