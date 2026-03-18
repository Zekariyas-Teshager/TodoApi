// Controllers/TodoItemsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApi.DTOs;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _todoItemService;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(
            ITodoItemService todoItemService,
            ILogger<TodoItemsController> logger)
        {
            _todoItemService = todoItemService;
            _logger = logger;
        }

        // GET: api/TodoItems
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TodoItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetTodoItems()
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            LogUserRoles();

            var todoItems = await _todoItemService.GetAllTodoItemsAsync(userId, isAdmin);
            return Ok(todoItems);
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TodoItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TodoItemResponseDto>> GetTodoItem(long id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            try
            {
                var todoItem = await _todoItemService.GetTodoItemByIdAsync(id, userId, isAdmin);
                
                if (todoItem == null)
                {
                    return NotFound(new { message = $"Todo item with ID {id} not found" });
                }

                return Ok(todoItem);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // POST: api/TodoItems
        [HttpPost]
        [ProducesResponseType(typeof(TodoItemResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TodoItemResponseDto>> PostTodoItem([FromBody] CreateTodoItemDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            
            var createdItem = await _todoItemService.CreateTodoItemAsync(createDto, userId);
            
            return CreatedAtAction(nameof(GetTodoItem), new { id = createdItem.Id }, createdItem);
        }

        // PATCH: api/TodoItems/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PatchTodoItem(long id, [FromBody] UpdateTodoItemDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            try
            {
                var result = await _todoItemService.UpdateTodoItemAsync(id, updateDto, userId, isAdmin);
                
                if (!result)
                {
                    return NotFound(new { message = $"Todo item with ID {id} not found" });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if item still exists
                var exists = await _todoItemService.TodoItemExistsAsync(id);
                if (!exists)
                {
                    return NotFound(new { message = $"Todo item with ID {id} was deleted during update" });
                }
                throw;
            }
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            try
            {
                var result = await _todoItemService.DeleteTodoItemAsync(id, userId, isAdmin);
                
                if (!result)
                {
                    return NotFound(new { message = $"Todo item with ID {id} not found" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        #region Private Helper Methods

        private string GetCurrentUserId()
        {
            return User.FindFirstValue("userId") ?? 
                   User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                   throw new UnauthorizedAccessException("User ID not found in token");
        }

        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        private void LogUserRoles()
        {
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            var roleList = string.Join(", ", roles);
            _logger.LogInformation("User roles: {Roles}", roleList);
        }

        #endregion
    }
}