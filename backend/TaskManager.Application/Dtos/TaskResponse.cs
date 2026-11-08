using System.Text.Json;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Dtos;

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TaskStatus Status,
    int UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    JsonElement? AdditionalData,
    string? AdditionalDataError);
