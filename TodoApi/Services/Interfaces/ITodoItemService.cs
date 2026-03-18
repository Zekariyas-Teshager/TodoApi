using TodoApi.DTOs;

namespace TodoApi.Services.Interfaces
{
    public interface ITodoItemService
    {
        Task<IEnumerable<TodoItemResponseDto>> GetAllTodoItemsAsync(string userId, bool isAdmin);
        Task<TodoItemResponseDto?> GetTodoItemByIdAsync(long id, string userId, bool isAdmin);
        Task<TodoItemResponseDto> CreateTodoItemAsync(CreateTodoItemDto createDto, string userId);
        Task<bool> UpdateTodoItemAsync(long id, UpdateTodoItemDto updateDto, string userId, bool isAdmin);
        Task<bool> DeleteTodoItemAsync(long id, string userId, bool isAdmin);
        Task<bool> TodoItemExistsAsync(long id);
    }
}