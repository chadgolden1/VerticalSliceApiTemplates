using FluentValidation;
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

        var todoListId = await sliceFixture.SendAsync(new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });

        var todoListsResponse = await sliceFixture.SendAsync(new GetTodoList.Query { TodoListId = todoListId });

        var todoList = todoListsResponse.TodoList.ShouldNotBeNull();
        todoList.Todos.ShouldContain(x => x.Name == todoName && x.Description == todoDescription);
    }

    [Fact]
    public async Task ShouldRejectEmptyTodoListName()
    {
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
        {
            await sliceFixture.SendAsync(new CreateTodoList.Command
            {
                Name = "",
                Todos =
                [
                    new() { Name = SampleName(), Description = SampleDescription() }
                ]
            });
        });

        exception.Message.ShouldContain("Name");
    }

    [Fact]
    public async Task ShouldRejectInvalidTodoFields()
    {
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
        {
            await sliceFixture.SendAsync(new CreateTodoList.Command
            {
                Name = SampleName(),
                Todos =
                [
                    new() { Name = SampleName(), Description = new string('a', 8001) },
                    new() { Name = "" },
                ]
            });
        });

        exception.Message.ShouldContain("Name");
        exception.Message.ShouldContain("Description");
    }


    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();
}
