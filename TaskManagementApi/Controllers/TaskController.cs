using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagementApi.Repositories.IRepositories;

namespace TaskManagementApi.Controllers
{
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
            var tasks = await repo.Get();
            if(!tasks.Any()) return NotFound();
            return Ok(tasks);
        }
    }
}
