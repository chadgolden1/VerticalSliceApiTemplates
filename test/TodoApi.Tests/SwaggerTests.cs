namespace TodoApi.Tests;

[Collection(nameof(SliceFixture))]
public class SwaggerTests(SliceFixture sliceFixture)
{
    [Fact]
    public async Task ShouldLoadSwaggerDoc()
    {
        var response = await sliceFixture.Client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
    }
}
