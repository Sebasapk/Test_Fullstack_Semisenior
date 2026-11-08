using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Dtos;
using TaskManager.Application.Interfaces;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService tasks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> GetAll(
        [FromQuery] TaskStatus? status, [FromQuery] int? userId, [FromQuery] string? priority) =>
        Ok(await tasks.GetAllAsync(status, userId, priority));

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var created = await tasks.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, UpdateTaskStatusRequest request) =>
        Ok(await tasks.UpdateStatusAsync(id, request));
}
