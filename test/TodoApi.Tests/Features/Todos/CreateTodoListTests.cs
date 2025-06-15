using System.Net.Http.Json;
using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class CreateTodoListTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldCreateTodoList()
    {
        string todoListName = SampleName();
        string todoName = SampleName();
        string todoDescription = SampleDescription();

        var createResponse = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTodoList.Response>();
        var todoListId = createResult!.TodoListId;

        var getResponse = await sliceFixture.Client.GetAsync($"/api/todos/list/{todoListId}");
        getResponse.EnsureSuccessStatusCode();
        var todoListsResponse = await getResponse.Content.ReadFromJsonAsync<GetTodoList.Response>();

        var todoList = todoListsResponse!.TodoList.ShouldNotBeNull();
        todoList.Todos.ShouldContain(x => x.Name == todoName && x.Description == todoDescription);
    }

    [Fact]
    public async Task ShouldRejectEmptyTodoListName()
    {
        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = "",
            Todos =
            [
                new() { Name = SampleName(), Description = SampleDescription() }
            ]
        });

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Name");
    }

    [Fact]
    public async Task ShouldRejectInvalidTodoFields()
    {
        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = SampleName(),
            Todos =
            [
                new() { Name = SampleName(), Description = new string('a', 8001) },
                new() { Name = "" },
            ]
        });

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Name");
        content.ShouldContain("Description");
    }


    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();
}
