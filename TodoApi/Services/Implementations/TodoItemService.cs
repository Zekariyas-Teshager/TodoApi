using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services.Implementations
{
    public class TodoItemService : ITodoItemService
    {
        private readonly TodoContext _context;
        private readonly ILogger<TodoItemService> _logger;

        public TodoItemService(TodoContext context, ILogger<TodoItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TodoItemResponseDto>> GetAllTodoItemsAsync(
            string userId,
            bool isAdmin
        )
        {
            IQueryable<TodoItem> query = _context.TodoItems;

            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);
            }

            var todoItems = await query.Select(t => MapToResponseDto(t)).ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} todo items for user {UserId} (IsAdmin: {IsAdmin})",
                todoItems.Count,
                userId,
                isAdmin
            );

            return todoItems;
        }

        public async Task<TodoItemResponseDto?> GetTodoItemByIdAsync(
            long id,
            string userId,
            bool isAdmin
        )
        {
            var todoItem = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);

            if (todoItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found", id);
                return null;
            }

            // Check permission
            if (!isAdmin && todoItem.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access todo item {Id} belonging to user {OwnerId}",
                    userId,
                    id,
                    todoItem.UserId
                );
                throw new UnauthorizedAccessException(
                    "You don't have permission to access this todo item"
                );
            }

            return MapToResponseDto(todoItem);
        }

        public async Task<TodoItemResponseDto> CreateTodoItemAsync(
            CreateTodoItemDto createDto,
            string userId
        )
        {
            var todoItem = new TodoItem
            {
                Name = createDto.Name,
                IsComplete = createDto.IsComplete,
                Secret = createDto.Secret,
                UserId = userId,
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Todo item created with ID {Id} for user {UserId}",
                todoItem.Id,
                userId
            );

            return MapToResponseDto(todoItem);
        }

        public async Task<bool> UpdateTodoItemAsync(
            long id,
            UpdateTodoItemDto updateDto,
            string userId,
            bool isAdmin
        )
        {
            if (id != updateDto.Id)
            {
                throw new ArgumentException("ID mismatch between URL and request body");
            }

            var existingItem = await _context.TodoItems.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for update", id);
                return false;
            }

            // Check permission
            if (!isAdmin && existingItem.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to update todo item {Id} belonging to user {OwnerId}",
                    userId,
                    id,
                    existingItem.UserId
                );
                throw new UnauthorizedAccessException(
                    "You don't have permission to update this todo item"
                );
            }

            // Update properties only if they are provided
            if (updateDto.Name != null)
            {
                existingItem.Name = updateDto.Name;
            }
            if (updateDto.IsComplete.HasValue)
            {
                existingItem.IsComplete = updateDto.IsComplete.Value;
            }
            if (updateDto.Secret != null)
            {
                existingItem.Secret = updateDto.Secret;
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Todo item {Id} updated successfully by user {UserId}",
                    id,
                    userId
                );
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating todo item {Id}", id);
                if (!await TodoItemExistsAsync(id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteTodoItemAsync(long id, string userId, bool isAdmin)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for deletion", id);
                return false;
            }

            // Check permission
            if (!isAdmin && todoItem.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to delete todo item {Id} belonging to user {OwnerId}",
                    userId,
                    id,
                    todoItem.UserId
                );
                throw new UnauthorizedAccessException(
                    "You don't have permission to delete this todo item"
                );
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Todo item {Id} deleted successfully by user {UserId}",
                id,
                userId
            );
            return true;
        }

        public async Task<bool> TodoItemExistsAsync(long id)
        {
            return await _context.TodoItems.AnyAsync(e => e.Id == id);
        }

        #region Private Helper Methods

        private static TodoItemResponseDto MapToResponseDto(TodoItem todoItem)
        {
            return new TodoItemResponseDto
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete,
            };
        }

        #endregion
    }
}
