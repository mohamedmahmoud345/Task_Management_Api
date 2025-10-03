using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTO;
using TaskManagement.Api.Enums;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Model;
using TaskManagement.Api.Repositories.IRepositories;

namespace TaskManagement.Tests.Controllers
{
    public class TaskControllerTest
    {
        private readonly string userId = "1";
        private readonly Mock<ITaskRepository> repo;
        private readonly Mock<ILogger<TaskController>> logger;
        private readonly TaskController controller;
        private readonly IMemoryCache cache;
        public TaskControllerTest()
        {

            repo = new Mock<ITaskRepository>();
            logger = new Mock<ILogger<TaskController>>();
            cache = new MemoryCache(new MemoryCacheOptions());
            controller = new TaskController(repo.Object, logger.Object , cache);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
               new Claim(ClaimTypes.NameIdentifier, "1")
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #region get endpoint
        [Fact]
        public async Task Get_WithValidId_ReturnOk()
        {
            var tasks =   new List<TaskData>
            {
                new TaskData{Id = 1 , Description = "new task" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title"},
                new TaskData{Id = 2 , Description = "new task 2" , DueDate = DateTime.Now.AddHours(2), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title 2"},
                new TaskData{Id = 3 , Description = "new task 3" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.MEDIUM ,Status = StatusEnum.Todo , Title = "task Title"},
            };

            repo.Setup(x => x.GetAsync(userId)).ReturnsAsync(tasks);

            var result = await controller.Get(1 , 5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Get_UserNotFound_ReturnsNotFound()
        {
            List<TaskData> tasks = [];

            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repo.Setup(x => x.GetAsync(userId)).ReturnsAsync(tasks);

            var result =await controller.Get(1 , 5);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task Get_NoTasks_ReturnsOkWithEmptyList()
        {
            List<TaskData> list = [];

            repo.Setup(x => x.GetAsync(userId)).ReturnsAsync(list);

            var result = await controller.Get(1, 5);

            var emptyList = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(list, emptyList.Value);
        }

        [Fact]
        public async Task Get_ExceptionThrown_Returns500()
        {

            repo.Setup(x => x.GetAsync(userId)).ThrowsAsync(new Exception());

            var result = await controller.Get(1, 5);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, objectResult.StatusCode);

        }
        #endregion

        #region start get by id endpoint

        [Fact]
        public async Task GetById_UserNotFound_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repo.Setup(x => x.GetByIdAsync(1, userId)).ReturnsAsync(new TaskData());

            var result = await controller.GetById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }
        [Fact]
        public async Task GetById_WithValidId_ReturnsOk()
        {
            var task = new TaskData { Id = 1, Description = "new task", DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.LOW, Status = StatusEnum.Todo, Title = "task Title" };

            repo.Setup(x => x.GetByIdAsync(1 , userId)).ReturnsAsync(task);

            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            TaskData task = null;

            repo.Setup(x => x.GetByIdAsync(1, userId)).ReturnsAsync(task);
            
            var result = await controller.GetById(1);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetById_ExceptionThrown_Returns500()
        {

            repo.Setup(x => x.GetByIdAsync(1, userId)).ThrowsAsync(new Exception());

            var result = await controller.GetById(1);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500 , objectResult.StatusCode);
        }
        #endregion

        #region start add endpoint
        [Fact]
        public async Task Add_InValidModelSatate_ReturnsBadRequest()
        {
            var task = new TaskDto();

            controller.ModelState.AddModelError("title" , "title is required");

            repo.Setup(x => x.AddAsync(task, userId)).ReturnsAsync((TaskData t, string u) => { t.Id = 1; return t; });

            var result = await controller.Add(task);

            var notFound = Assert.IsType<BadRequestResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task Add_UserNotFound_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var task = new TaskDto();

            repo.Setup(x => x.AddAsync(task, userId)).ReturnsAsync((TaskData t , string u) => { t.Id = 1; return t; });

            var result = await controller.Add(task);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]  
        public async Task Add_TaskCreatedSuccesfully_ReturnsCreatedAdAction()
        {
            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.AddAsync(It.IsAny<TaskDto>(), userId))
             .ReturnsAsync((TaskDto dto, string u) =>
             {
                 var taskData = dto.ToModel(u);
                 taskData.Id = 1;
                 return taskData;
             });

            var result = await controller.Add(task);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult);
        }
        [Fact]
        public async Task Add_ExceptionThrown_Returns500()
        {
            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.AddAsync(task, userId)).ThrowsAsync(new Exception());

            var result = await controller.Add(task);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
        #endregion

        #region start edit endpoint 

        [Fact]
        public async Task Edit_InValidModelStatae_ReturnsBadRequest()
        {
            controller.ModelState.AddModelError("title", "title is requered");

            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.EditAsync(task, userId)).ReturnsAsync(false);

            var result = await controller.Edit(1, task);

            var badRequest = Assert.IsType<BadRequestResult>(result);
            Assert.NotNull(badRequest);
        }

        [Fact]
        public async Task Edit_InValidUser_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.EditAsync(task, userId)).ReturnsAsync(false);

            var result = await controller.Edit(1, task);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task Edit_ValidData_ReturnsNoContent()
        {
            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.EditAsync(task, userId)).ReturnsAsync(true);

            var result = await controller.Edit(1, task);

            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContent);
        }
        [Fact]
        public async Task Edit_ExceptionThrown_Returns500()
        {
            var task = new TaskData { Id = 1, Description = "new task", Title = "task Title" }.ToDto();

            repo.Setup(x => x.EditAsync(task, userId)).ThrowsAsync(new Exception());

            var result = await controller.Edit(1, task);

            var internalServerError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerError.StatusCode);
        }
        #endregion

        #region start delete endpoint 

        [Fact]
        public async Task Delete_InValidUserId_ReturnsNotFound()
        {

            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repo.Setup(x => x.RemoveAsync(1, userId)).ReturnsAsync(false);

            var result = await controller.Delete(1);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task Delete_WithUnexictTask_ReturnsNotFound()
        {
            repo.Setup(x => x.RemoveAsync(1, userId)).ReturnsAsync(false);

            var result = await controller.Delete(1);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task Delete_Valid_ReturnsNoContent()
        {
            repo.Setup(x => x.RemoveAsync(1, userId)).ReturnsAsync(true);

            var result = await controller.Delete(1);

            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContent);
        }
        [Fact]
        public async Task Delete_ExceptionThrown_Returns500()
        {
            repo.Setup(x => x.RemoveAsync(1, userId)).ThrowsAsync(new Exception());

            var result = await controller.Delete(1);

            var internalServerError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerError.StatusCode);
        }
        #endregion

        #region start get by status endpoint 

        [Fact]
        public async Task GetByStatus_InValidStatusValue_ReturnsBadRequest()
        {
            var inValidStatusNumber = 4;

            repo.Setup(x => x.FilterByStatus(inValidStatusNumber, userId)).ReturnsAsync([]);

            var result = await controller.GetByStatus(inValidStatusNumber);

            var badRequest = Assert.IsType<BadRequestResult>(result);
            Assert.NotNull(badRequest);
        }

        [Fact]
        public async Task GetByStatus_WithInValidUser_ReturnsNotFound()
        {
            int statusNumber = 2;

            var user = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repo.Setup(x => x.FilterByStatus(statusNumber, userId));

            var result = await controller.GetByStatus(statusNumber);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task GetByStatus_WithUnexictTasks_ReturnsNotFound()
        {
            int statusNumber = 2;

            repo.Setup(x => x.FilterByStatus(statusNumber, userId)).ReturnsAsync([]);

            var result = await controller.GetByStatus(statusNumber);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetByStatus_WithValidRequest_ReturnsOkWithData()
        {
            var tasks = new List<TaskData>
            {
                new TaskData{Id = 1 , Description = "new task" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title"},
                new TaskData{Id = 2 , Description = "new task 2" , DueDate = DateTime.Now.AddHours(2), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title 2"},
                new TaskData{Id = 3 , Description = "new task 3" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.MEDIUM ,Status = StatusEnum.Todo , Title = "task Title"},
            };

            var statusNumber = 0;

            repo.Setup(x => x.FilterByStatus(statusNumber, userId)).ReturnsAsync(tasks);

            var result = await controller.GetByStatus(statusNumber);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }
        [Fact]
        public async Task GetByStatus_ExceptionThrown_Returns500()
        {
            repo.Setup(x => x.FilterByPriority(1, userId)).ThrowsAsync(new Exception());

            var result = await controller.GetByStatus(1);

            var internalServerError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerError.StatusCode);
        }
        #endregion

        #region start get by priority endpoint 

        [Fact]
        public async Task GetByPriority_WithInvalidPriorityValue_ReturnsBadRequest()
        {
            var priorityValue = 4;
            repo.Setup(x => x.FilterByPriority(priorityValue, userId)).ReturnsAsync([]);

            var result = await controller.GetByPriority(priorityValue);

            var badRequest = Assert.IsType<BadRequestResult>(result);
            Assert.NotNull(badRequest);                  
        }
        [Fact]
        public async Task GetByPriority_WithInValidUser_ReturnsNotFound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            int priorityNumber = 3;

            repo.Setup(x => x.FilterByPriority(priorityNumber , userId)).ReturnsAsync([]);

            var result = await controller.GetByPriority(priorityNumber);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetByPriority_WithUnexictTasks_ReturnsNotFound()
        {
            int priorityNumber = 3;
            repo.Setup(x => x.FilterByPriority(priorityNumber, userId)).ReturnsAsync([]);

            var result = await controller.GetByPriority(priorityNumber);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task GetByPriority_ValidData_ReturnsOk()
        {
            var tasks = new List<TaskData>
            {
                new TaskData{Id = 1 , Description = "new task" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title"},
                new TaskData{Id = 2 , Description = "new task 2" , DueDate = DateTime.Now.AddHours(2), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title 2"},
                new TaskData{Id = 3 , Description = "new task 3" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.MEDIUM ,Status = StatusEnum.Todo , Title = "task Title"},
            };
            int priorityValud = 2;

            repo.Setup(x => x.FilterByPriority(priorityValud, userId)).ReturnsAsync(tasks);

            var result = await controller.GetByPriority(priorityValud);    

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }
        [Fact]
        public async Task GetByPriority_ExceptionThrown_Returns500()
        {
            int priorityValue = 1;
            repo.Setup(x => x.FilterByPriority(priorityValue, userId)).ThrowsAsync(new Exception());

            var result = await controller.GetByPriority(priorityValue);

            var interenalServerError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, interenalServerError.StatusCode);
        }
        #endregion

        #region start Search By Title endpoint

        [Fact]
        public async Task SearchByTitle_WithInvalidUser_ReturnsNotfound()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var title = "this is title";
            repo.Setup(x => x.SearchByTitle(title, userId)).ReturnsAsync([]);

            var result = await controller.SearchByTitle(title);

            var notfound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notfound);
        }
        [Fact]
        public async Task SearchByTitle_WithUnexictTasks_ReturnsNotFound()
        {
            var title = "title";

            repo.Setup(x => x.SearchByTitle(title, userId)).ReturnsAsync([]);

            var result = await controller.SearchByTitle(title);

            var notFound = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFound);
        }
        [Fact]
        public async Task SearchBytitle_ValidData_ReturnsOk()
        {
            var tasks = new List<TaskData>
            {
                new TaskData{Id = 1 , Description = "new task" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title"},
                new TaskData{Id = 2 , Description = "new task 2" , DueDate = DateTime.Now.AddHours(2), Priority = PriorityEnum.LOW ,Status = StatusEnum.Todo , Title = "task Title"},
                new TaskData{Id = 3 , Description = "new task 3" , DueDate = DateTime.Now.AddHours(1), Priority = PriorityEnum.MEDIUM ,Status = StatusEnum.Todo , Title = "task Title"},
            };

            var title = "task Title";

            repo.Setup(x => x.SearchByTitle(title.ToLower(), userId)).ReturnsAsync(tasks);

            var result = await controller.SearchByTitle(title.ToLower());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }
        [Fact]
        public async Task SearchByTitle_ExceptionThrown_Returns500()
        {
            var title = "title";
            repo.Setup(x => x.SearchByTitle(title, userId)).ThrowsAsync(new Exception());

            var ressult = await controller.SearchByTitle(title);

            var internalServerError = Assert.IsType<ObjectResult>(ressult);
            Assert.Equal(500, internalServerError.StatusCode);
        }
        #endregion
    }
}
