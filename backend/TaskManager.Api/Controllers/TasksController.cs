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
    [ProducesResponseType<List<TaskResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TaskResponse>>> GetAll(
        [FromQuery] TaskStatus? status, [FromQuery] int? userId, [FromQuery] string? priority) =>
        Ok(await tasks.GetAllAsync(status, userId, priority));

    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var created = await tasks.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, UpdateTaskStatusRequest request) =>
        Ok(await tasks.UpdateStatusAsync(id, request));
}
