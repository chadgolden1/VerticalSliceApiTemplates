using System.Net.Http.Json;
using Shouldly;
using TodoApi.Features.Todos;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class ListTodoListTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldListTodoLists()
    {
        string todoListName = SampleName();
        string todoName = SampleName();
        string todoDescription = SampleDescription();

        var createResponse1 = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });
        createResponse1.EnsureSuccessStatusCode();

        string todoListName2 = SampleName();
        string todoName2 = SampleName();
        string todoDescription2 = SampleDescription();
        string todoName3 = SampleName();
        string todoDescription3 = SampleDescription();

        var createResponse2 = await sliceFixture.Client.PostAsJsonAsync("/api/todos/list", new CreateTodoList.Command
        {
            Name = todoListName2,
            Todos =
            [
                new() { Name = todoName2, Description = todoDescription2 },
                new() { Name = todoName3, Description = todoDescription3 },
            ]
        });
        createResponse2.EnsureSuccessStatusCode();

        var response = await sliceFixture.Client.GetAsync("/api/todos/list");
        response.EnsureSuccessStatusCode();

        var todoListsResponse = await response.Content.ReadFromJsonAsync<ListTodoLists.Response>();

        todoListsResponse.ShouldNotBeNull();
        todoListsResponse.TodoLists.ShouldNotBeEmpty();
        todoListsResponse.TodoLists.First(x => x.Name == todoListName).Todos.ShouldContain(x => x.Name == todoName && x.Description == todoDescription);
        todoListsResponse.TodoLists.First(x => x.Name == todoListName2).Todos.ShouldContain(x => x.Name == todoName2 && x.Description == todoDescription2);
        todoListsResponse.TodoLists.First(x => x.Name == todoListName2).Todos.ShouldContain(x => x.Name == todoName3 && x.Description == todoDescription3);
    }

    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();
}
