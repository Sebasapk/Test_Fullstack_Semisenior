using TaskManager.Domain.Entities;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(TaskStatus? status, int? userId, string? priority);
    Task<TaskItem?> GetByIdAsync(int id);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
}
