using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Model;
using TaskManagement.Api.Repositories.IRepositories;

namespace TaskManagement.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing tasks
    /// </summary>
    /// <remarks>This controller handles task-related operations for the authenticated user.  All endpoints
    /// require the user to be authorized and authenticated.  Tasks are scoped to the current user, and operations will
    /// only affect tasks owned by the user.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository repo;
        private readonly ILogger<TaskController> logger;
        public TaskController(ITaskRepository repo, ILogger<TaskController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            return userId;
        }

        /// <summary>
        /// Retrieves a paginated list of tasks for the authenticated user
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (default: 1)</param>
        /// <param name="pageSize">The number of tasks per page (default: 5)</param>
        /// <returns>A paginated list of task DTOs</returns>
        /// <response code="200">Returns the list of tasks</response>
        /// <response code="404">No tasks found for the user</response>
        /// <response code="500">An error occurred while retrieving tasks</response>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageNumber=1 , [FromQuery] int pageSize = 5)
        {
            try
            {
                var userId = GetCurrentUserId();
                if(userId == null)
                    return NotFound("User Not Found");

                var tasks = await repo.GetAsync(userId);

                if(!tasks.Any()) return NotFound();

                return Ok(Pagination(tasks , pageNumber , pageSize));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Retrieving Tasks");
                return StatusCode(500, "An Error Occurred whlie retrieving tasks");
            }
        }
        /// <summary>
        /// Retrieves a specific task by ID for the authenticated user
        /// </summary>
        /// <param name="id">The ID of the task to retrieve</param>
        /// <returns>The task DTO if found</returns>
        /// <response code="200">Returns the requested task</response>
        /// <response code="404">Task not found or access denied</response>
        /// <response code="500">An error occurred while retrieving the task</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound("User Not Found");

                var task = await repo.GetByIdAsync(id , userId);
                if(task == null) return NotFound();

                return Ok(task.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task");
            }   
        }
        /// <summary>
        /// Creates a new task for the authenticated user
        /// </summary>
        /// <param name="data">The task data to create</param>
        /// <returns>The created task DTO</returns>
        /// <response code="201">Task created successfully</response>
        /// <response code="400">Invalid task data provided</response>
        /// <response code="500">An error occurred while creating the task</response>
        [HttpPost]
        public async Task<IActionResult> Add(TaskDto data)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var userId = GetCurrentUserId();

                if (userId == null) return NotFound();
                data.Id = 0;

                var task = await repo.AddAsync(data , userId);
                await repo.SaveAsync();

                return CreatedAtAction(nameof(GetById) , new {id = task.Id } , task.ToDto());
            }
            catch(Exception ex)
            {
                logger.LogError("Error Creating Task");
                return StatusCode(500, "an error occurred while creating new task");
            }
        }
        /// <summary>
        /// Updates an existing task for the authenticated user
        /// </summary>
        /// <param name="id">The ID of the task to update</param>
        /// <param name="data">The updated task data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Task updated successfully</response>
        /// <response code="400">Invalid task data or ID mismatch</response>
        /// <response code="404">Task not found or access denied</response>
        /// <response code="500">An error occurred while updating the task</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id , TaskDto data)
        {
            if (id != data.Id || !ModelState.IsValid) return BadRequest();

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound();
                var success = await repo.EditAsync(data , userId);
                if (success != true)
                    return NotFound($"Task With Id {id} not found or access denied");
                await repo.SaveAsync();

                return NoContent();
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task");
            }
        }
        /// <summary>
        /// Deletes a task for the authenticated user
        /// </summary>
        /// <param name="id">The ID of the task to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Task deleted successfully</response>
        /// <response code="404">Task not found or access denied</response>
        /// <response code="500">An error occurred while deleting the task</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) 
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound();
                var success = await repo.RemoveAsync(id , userId);

                if (success != true)
                    return NotFound();

                await repo.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }
        /// <summary>
        /// Retrieves tasks filtered by status for the authenticated user
        /// </summary>
        /// <param name="statusNumber">The status number to filter by (0-3)</param>
        /// <param name="pageNumber">The page number to retrieve (default: 1)</param>
        /// <param name="pageSize">The number of tasks per page (default: 5)</param>
        /// <returns>A paginated list of filtered task DTOs</returns>
        /// <response code="200">Returns the filtered list of tasks</response>
        /// <response code="400">Invalid status number provided</response>
        /// <response code="404">No tasks found with the specified status</response>
        /// <response code="500">An error occurred while filtering tasks</response>
        [HttpGet("filter/status/{statusNumber:int}")]
        public async Task<IActionResult> GetByStatus
            (int statusNumber , [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                if(statusNumber < 0 || statusNumber > 3) return BadRequest();
                
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound();

                var tasks = await repo.FilterByStatus(statusNumber, userId);

                if (!tasks.Any())
                    return NotFound();

                return Ok(Pagination(tasks , pageNumber , pageSize));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on filtartion by status");
                return StatusCode(500, "an error occured while status filtration");
            }
        }
        /// <summary>
        /// Retrieves tasks filtered by priority for the authenticated user
        /// </summary>
        /// <param name="priorityNumber">The priority number to filter by (0-3)</param>
        /// <param name="pageNumber">The page number to retrieve (default: 1)</param>
        /// <param name="pageSize">The number of tasks per page (default: 5)</param>
        /// <returns>A paginated list of filtered task DTOs</returns>
        /// <response code="200">Returns the filtered list of tasks</response>
        /// <response code="400">Invalid priority number provided</response>
        /// <response code="404">No tasks found with the specified priority</response>
        /// <response code="500">An error occurred while filtering tasks</response>
        [HttpGet("filter/priority/{priorityNumber:int}")]
        public async Task<IActionResult> GetByPriority
            (int priorityNumber , [FromQuery] int pageNumber = 1 , [FromQuery] int pageSize = 5)
        {
            try
            {
                if(priorityNumber < 0 || priorityNumber > 3) return BadRequest();
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound();

                var tasks = await repo.FilterByPriority(priorityNumber, userId);

                if (!tasks.Any())
                    return NotFound();

                return Ok(Pagination(tasks, pageNumber , pageSize));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on filtartion by priority");
                return StatusCode(500, "an error occured while priority filtration");
            }
        }
        /// <summary>
        /// Searches tasks by title for the authenticated user
        /// </summary>
        /// <param name="title">The title search term</param>
        /// <param name="pageNumber">The page number to retrieve (default: 1)</param>
        /// <param name="pageSize">The number of tasks per page (default: 5)</param>
        /// <returns>A paginated list of matching task DTOs</returns>
        /// <response code="200">Returns the search results</response>
        /// <response code="404">No tasks found matching the search term</response>
        /// <response code="500">An error occurred while searching tasks</response>
        [HttpGet("search/{title}")]
        public async Task<IActionResult> SearchByTitle
            (string title , [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return NotFound();

                var tasks = await repo.SearchByTitle(title.ToLower(), userId);

                if (!tasks.Any())
                    return NotFound();

                return Ok(Pagination(tasks, pageNumber , pageSize));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "error on search action");
                return StatusCode(500, "An error occured while searching");
            }
        }

        private List<TaskDto> Pagination
            (List<TaskData> tasks , int pageNumber , int pageSize)
        {
            var count = tasks.Count();

            var pageNumbers = (pageNumber - 1) * pageSize;

            var result = tasks.Skip(pageNumbers)
                .Take(pageSize)
                .ToList();

            return result.ToDto();
        }
    }
}
