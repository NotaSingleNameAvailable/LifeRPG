using LifeRPG.API.DTOs;
using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetTasks(Guid userId)
        {
            var tasks = await _taskService.GetTasksAsync(userId);
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var (result, error) = await _taskService.CreateTaskAsync(dto);

            if (result == null)
                return BadRequest(error);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] CreateTaskDto dto)
        {
            var (success, error) = await _taskService.UpdateTaskAsync(id, dto);

            if (!success)
            {
                if (error == "NotFound") return NotFound();
                return BadRequest(error);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var (success, error) = await _taskService.DeleteTaskAsync(id);

            if (!success)
            {
                if (error == "NotFound") return NotFound();
                return BadRequest(error);
            }

            return Ok();
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(Guid id)
        {
            var (result, error) = await _taskService.CompleteTaskAsync(id);

            if (result == null)
            {
                if (error == "NotFound") return NotFound();
                return BadRequest(error);
            }

            return Ok(result);
        }
    }
}