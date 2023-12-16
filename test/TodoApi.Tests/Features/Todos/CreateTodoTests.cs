using FluentValidation;
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
        var todoId = await sliceFixture.SendAsync(new CreateTodo.Command
        {
            TodoListId = todoListId,
            Name = newTodoName,
            Description = newTodoDescription
        });

        var getResponse = await sliceFixture.SendAsync(new GetTodoList.Query { TodoListId = todoListId });

        var todoList = getResponse.TodoList.ShouldNotBeNull();
        todoList.Todos.Count.ShouldBe(2);
        todoList.Todos.First(x => x.Name == newTodoName).Description.ShouldBe(newTodoDescription);
    }

    [Fact]
    public async Task ShouldRejectEmptyName()
    {
        int todoListId = await CreateTodoList();

        var exception = await Should.ThrowAsync<ValidationException>(async () =>
        {
            await sliceFixture.SendAsync(new CreateTodo.Command
            {
                TodoListId = todoListId,
                Name = "",
            });
        });

        exception.Message.ShouldContain("Name");
        exception.Message.ShouldNotContain("Description");
    }

    [Fact]
    public async Task ShouldRejectFieldsOverMaxLength()
    {
        int todoListId = await CreateTodoList();

        var exception = await Should.ThrowAsync<ValidationException>(async () =>
        {
            await sliceFixture.SendAsync(new CreateTodo.Command
            {
                TodoListId = todoListId,
                Name = new string('a', 101),
                Description = new string('a', 8001)
            });
        });

        exception.Message.ShouldContain("Name");
        exception.Message.ShouldContain("Description");
    }

    [Fact]
    public async Task ShouldRejectNonExistingTodoListId()
    {
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
        {
            await sliceFixture.SendAsync(new CreateTodo.Command
            {
                TodoListId = -8236212,
                Name = SampleName(),
                Description = SampleDescription()
            });
        });

        exception.Message.ShouldContain("does not exist");
    }

    private static string SampleName() => SampleData.SampleString();
    private static string SampleDescription() => SampleData.SampleString();

    private async Task<int> CreateTodoList()
    {
        string todoListName = SampleName();
        string todoName = SampleName();
        string todoDescription = SampleDescription();

        return await sliceFixture.SendAsync(new CreateTodoList.Command
        {
            Name = todoListName,
            Todos =
            [
                new() { Name = todoName, Description = todoDescription }
            ]
        });
    }
}
