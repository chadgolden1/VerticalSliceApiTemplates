using System.Net;
using Shouldly;

namespace TodoApi.Tests.Features.Todos;

[Collection(nameof(SliceFixture))]
public class GetTodoListTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldReturnNotFoundForTodoListThatDoesNotExist()
    {
        int todoListIdThatDoesNotExist = -23464;

        var response = await sliceFixture.Client.GetAsync($"/api/todos/list/{todoListIdThatDoesNotExist}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
