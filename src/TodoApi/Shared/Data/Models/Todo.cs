namespace TodoApi.Shared.Data.Models;

public class Todo
{
    public int TodoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }

    public int TodoListId { get; set; }
    public TodoList TodoList { get; set; } = new TodoList();
}
