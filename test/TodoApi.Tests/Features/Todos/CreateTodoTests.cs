using System.Net.Http.Json;
using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class CreateTodoTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldAddTodoToExistingTodoList()
    {
        int todoListId = await CreateTodoList();

        string newTodoName = SampleName();
        string newTodoDescription = SampleDescription();

        var createResponse = await sliceFixture.Client.PostAsJsonAsync("/api/todos", new CreateTodo.Command
        {
            TodoListId = todoListId,
            Name = newTodoName,
            Description = newTodoDescription
        });
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await sliceFixture.Client.GetAsync($"/api/todos/list/{todoListId}");
        getResponse.EnsureSuccessStatusCode();
        var getTodoListResponse = await getResponse.Content.ReadFromJsonAsync<GetTodoList.Response>();

        var todoList = getTodoListResponse!.TodoList.ShouldNotBeNull();
        todoList.Todos.Count.ShouldBe(2);
        todoList.Todos.First(x => x.Name == newTodoName).Description.ShouldBe(newTodoDescription);
    }

    [Fact]
    public async Task ShouldRejectEmptyName()
    {
        int todoListId = await CreateTodoList();

        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos", new CreateTodo.Command
        {
            TodoListId = todoListId,
            Name = "",
        });

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Name");
        content.ShouldNotContain("Description");
    }

    [Fact]
    public async Task ShouldRejectFieldsOverMaxLength()
    {
        int todoListId = await CreateTodoList();

        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos", new CreateTodo.Command
        {
            TodoListId = todoListId,
            Name = new string('a', 101),
            Description = new string('a', 8001)
        });

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Name");
        content.ShouldContain("Description");
    }

    [Fact]
    public async Task ShouldRejectNonExistingTodoListId()
    {
        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos", new CreateTodo.Command
        {
            TodoListId = -8236212,
            Name = SampleName(),
            Description = SampleDescription()
        });

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("does not exist");
    }

    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();

    private async Task<int> CreateTodoList()
    {
        string todoListName = SampleName();
        string todoName = SampleName();
        string todoDescription = SampleDescription();

        var response = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });
        response.EnsureSuccessStatusCode();
        var createResponse = await response.Content.ReadFromJsonAsync<CreateTodoList.Response>();
        return createResponse!.TodoListId;
    }
}
