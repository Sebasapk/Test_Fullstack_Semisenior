using System.Text.Json;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService(ITaskRepository tasks, IUserRepository users) : ITaskService
{
    public async Task<List<TaskResponse>> GetAllAsync(TaskStatus? status, int? userId)
    {
        var all = await tasks.GetAllAsync(status, userId);
        return all.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        var title = request.Title?.Trim();
        if (string.IsNullOrEmpty(title))
            throw new BusinessRuleException("Title is required.");

        var user = await users.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User {request.UserId} was not found.");

        var task = new TaskItem
        {
            Title = title,
            Description = request.Description?.Trim(),
            UserId = user.Id,
            Status = TaskStatus.Pending,
            AdditionalData = request.AdditionalData is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined }
                ? request.AdditionalData.Value.GetRawText()
                : null
        };
        await tasks.AddAsync(task);

        task.User = user;
        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateStatusAsync(int id, UpdateTaskStatusRequest request)
    {
        var task = await tasks.GetByIdAsync(id)
            ?? throw new NotFoundException($"Task {id} was not found.");

        var newStatus = request.Status!.Value;

        if (task.Status == TaskStatus.Pending && newStatus == TaskStatus.Done)
            throw new BusinessRuleException("A task cannot go from Pending to Done directly; it must be InProgress first.");

        task.Status = newStatus;
        task.UpdatedAt = DateTime.UtcNow;
        await tasks.UpdateAsync(task);

        return ToResponse(task);
    }

    private static TaskResponse ToResponse(TaskItem task)
    {
        var (additionalData, error) = ParseAdditionalData(task.AdditionalData);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.UserId,
            task.User.Name,
            task.CreatedAt,
            task.UpdatedAt,
            additionalData,
            error);
    }

    private static (JsonElement? Data, string? Error) ParseAdditionalData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return (null, null);

        try
        {
            using var document = JsonDocument.Parse(json);
            return (document.RootElement.Clone(), null);
        }
        catch (JsonException)
        {
            return (null, "AdditionalData is not valid JSON and could not be read.");
        }
    }
}
