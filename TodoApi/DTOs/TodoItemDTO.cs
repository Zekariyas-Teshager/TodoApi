namespace TodoApi.DTOs
{
    public class CreateTodoItemDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string? Secret { get; set; }
    }

    public class UpdateTodoItemDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public bool? IsComplete { get; set; }
        public string? Secret { get; set; }
    }

    public class TodoItemResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
    }
}