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

        var todoListId = await sliceFixture.SendAsync(new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });

        string todoListName2 = SampleName();
        string todoName2 = SampleName();
        string todoDescription2 = SampleDescription();
        string todoName3 = SampleName();
        string todoDescription3 = SampleDescription();

        var todoListId2 = await sliceFixture.SendAsync(new CreateTodoList.Command
        {
            Name = todoListName2,
            Todos =
            [
                new() { Name = todoName2, Description = todoDescription2 },
                new() { Name = todoName3, Description = todoDescription3 },
            ]
        });

        var todoListsResponse = await sliceFixture.SendAsync(new ListTodoLists.Query());

        todoListsResponse.TodoLists.ShouldNotBeEmpty();
        todoListsResponse.TodoLists.First(x => x.Name == todoListName).Todos.ShouldContain(x => x.Name == todoName && x.Description == todoDescription);
        todoListsResponse.TodoLists.First(x => x.Name == todoListName2).Todos.ShouldContain(x => x.Name == todoName2 && x.Description == todoDescription2);
        todoListsResponse.TodoLists.First(x => x.Name == todoListName2).Todos.ShouldContain(x => x.Name == todoName3 && x.Description == todoDescription3);
    }

    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();
}
