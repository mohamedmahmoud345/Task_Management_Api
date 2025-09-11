using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.DTO;
using TaskManagementApi.Repositories.IRepositories;

namespace TaskManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private ITaskRepository repo;
        public TaskController(ITaskRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tasks = await repo.GetAsync();
            if(!tasks.Any()) return NotFound();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await repo.GetByIdAsync(id);
            if(task == null) return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Add(TaskDto data)
        {
            if (!ModelState.IsValid) return BadRequest();

            data.Id = 0;

            await repo.AddAsync(data);
            await repo.SaveAsync();


            return Created();
        }

        [HttpPut]
        public async Task<IActionResult> Edit(int id , TaskDto data)
        {
            if (id != data.Id || !ModelState.IsValid) return BadRequest();

            await repo.EditAsync(data);
            await repo.SaveAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id) 
        {
            var task = await repo.GetByIdAsync(id);
            if(task == null) return BadRequest();

            await repo.RemoveAsync(id);
            await repo.SaveAsync();

            return NoContent();
        }
    }
}
