namespace TodoApi.Shared.Data.Models;

public class TodoList
{
    public int TodoListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Todo> Todos { get; set; } = [];
}
