using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
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
    [EnableRateLimiting("per-user")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository repo;
        private readonly ILogger<TaskController> logger;
        private readonly IMemoryCache cache;
        public TaskController(ITaskRepository repo, ILogger<TaskController> logger , IMemoryCache cache)
        {
            this.repo = repo;
            this.logger = logger;
            this.cache = cache;
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
            var userId = GetCurrentUserId();
            var casheKey = $"tasks{userId}";
            
            if(userId == null)
                return NotFound("User Not Found");


            if(!cache.TryGetValue(casheKey , out List<TaskData> tasks))
            {
                tasks = await repo.GetAsync(userId);

                var cacheEntryOptions =
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                cache.Set(casheKey, tasks , cacheEntryOptions);
            }

            var pagedTasks = Pagination(tasks, pageNumber, pageSize);

            return Ok(pagedTasks);
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
            var userId = GetCurrentUserId();
            if (userId == null) return NotFound("User Not Found");

            var task = await repo.GetByIdAsync(id , userId);
            if(task == null) return NotFound();

            return Ok(task.ToDto());
            
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


            var userId = GetCurrentUserId();

            if (userId == null) return NotFound();
            data.Id = 0;

            var task = await repo.AddAsync(data , userId);
            await repo.SaveAsync();

            return CreatedAtAction(nameof(GetById) , new {id = task.Id } , task.ToDto());
  
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

            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();
            var success = await repo.EditAsync(data , userId);
            if (success != true)
                return NotFound($"Task With Id {id} not found or access denied");
            await repo.SaveAsync();

            return NoContent();

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
            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();
            var success = await repo.RemoveAsync(id , userId);

            if (success != true)
                return NotFound();

            await repo.SaveAsync();

            return NoContent();

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
            if(statusNumber < 0 || statusNumber > 3) return BadRequest();
            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();

            var casheKey = $"get by stastus {userId}";

            if(!cache.TryGetValue(casheKey , out List<TaskData> tasks))
            {
                tasks = await repo.FilterByStatus(statusNumber, userId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); 

                cache.Set(casheKey, tasks, cacheEntryOptions);
            }

            var pagination = Pagination(tasks, pageNumber, pageSize);

            return Ok(pagination);

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
            if(priorityNumber < 0 || priorityNumber > 3) return BadRequest();
            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();
            var cacheKey = $"get by priority {userId}";

            if(!cache.TryGetValue(cacheKey , out List<TaskData> tasks))
            {
                tasks = await repo.FilterByPriority(priorityNumber, userId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                cache.Set(cacheKey, tasks, cacheEntryOptions);
            }

            var pagination = Pagination(tasks, pageNumber, pageSize);
            return Ok(pagination);

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

            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();
            var cacheKey = $"search by title {userId}";

            if(!cache.TryGetValue(cacheKey , out List<TaskData> tasks))
            {
                tasks = await repo.SearchByTitle(title.ToLower(), userId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                cache.Set(cacheKey, tasks, cacheEntryOptions);
            }

            var pagination = Pagination(tasks, pageNumber, pageSize);

            return Ok(pagination);

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
