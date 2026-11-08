using TaskManager.Application.Dtos;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(TaskStatus? status, int? userId);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);
    Task<TaskResponse> UpdateStatusAsync(int id, UpdateTaskStatusRequest request);
}
