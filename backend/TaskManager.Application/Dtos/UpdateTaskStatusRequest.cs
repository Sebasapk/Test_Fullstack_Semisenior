using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Dtos;

public class UpdateTaskStatusRequest
{
    // Nullable so a missing field fails [Required] instead of silently becoming Pending (the enum's 0 value).
    [Required, EnumDataType(typeof(TaskStatus))]
    public TaskStatus? Status { get; set; }
}
