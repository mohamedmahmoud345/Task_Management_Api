using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementApi.DTO;
using TaskManagementApi.Repositories.IRepositories;
using TaskManagementApi.Extensions;
using Microsoft.AspNetCore.Authorization;


namespace TaskManagementApi.Controllers
{
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
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User Id Not Found");

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await repo.GetAsync(userId);
                if(!tasks.Any()) return NotFound();
                return Ok(tasks.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Retrieving Tasks");
                return StatusCode(500, "An Error Occurred whlie retrieving tasks");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
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

        [HttpPost]
        public async Task<IActionResult> Add(TaskDto data)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var userId = GetCurrentUserId();
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id , TaskDto data)
        {
            if (id != data.Id || !ModelState.IsValid) return BadRequest();

            try
            {
                var userId = GetCurrentUserId();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) 
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await repo.RemoveAsync(id , userId);

                if (success != true)
                    return NotFound($"Task with ID {id} not found or access denied");

                await repo.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }
    }
}
