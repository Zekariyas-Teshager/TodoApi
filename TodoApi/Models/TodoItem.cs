namespace TodoApi.Models;

public class TodoItem
{
    public long Id { get; set; }

    /// <summary>
    /// The title of the todo item
    /// </summary>
    /// <example>Buy groceries</example>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the task is completed
    /// </summary>
    /// <example>false</example>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Secret notes about the task (not displayed in list views)
    /// </summary>
    /// <example>Remember to buy milk and eggs</example>
    public string? Secret { get; set; }

    public string? UserId { get; set; }
}
