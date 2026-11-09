using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Tests.Fakes;

public class FakeTaskRepository : ITaskRepository
{
    public List<TaskItem> Items { get; } = [];
    public int UpdateCalls { get; private set; }

    public Task<List<TaskItem>> GetAllAsync(TaskStatus? status, int? userId, string? priority) =>
        Task.FromResult(Items
            .Where(t => status is null || t.Status == status)
            .Where(t => userId is null || t.UserId == userId)
            .ToList());

    public Task<TaskItem?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public Task AddAsync(TaskItem task)
    {
        task.Id = Items.Count + 1;
        Items.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task)
    {
        UpdateCalls++;
        return Task.CompletedTask;
    }
}
