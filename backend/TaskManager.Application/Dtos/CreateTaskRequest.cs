using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.Dtos;

public class CreateTaskRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    public TaskAdditionalDataDto? AdditionalData { get; set; }
}
