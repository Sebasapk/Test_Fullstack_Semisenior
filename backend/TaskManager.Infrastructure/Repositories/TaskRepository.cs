using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public Task<List<TaskItem>> GetAllAsync(TaskStatus? status, int? userId, string? priority)
    {
        var query = db.Tasks.AsNoTracking().Include(t => t.User).AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => AppDbContext.JsonValue(t.AdditionalData, "$.priority") == priority);

        return query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public Task<TaskItem?> GetByIdAsync(int id) =>
        db.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(TaskItem task)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(TaskItem task) => db.SaveChangesAsync();
}
