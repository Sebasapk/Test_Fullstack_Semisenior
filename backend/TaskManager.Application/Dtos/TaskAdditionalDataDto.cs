using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.Dtos;

public class TaskAdditionalDataDto
{
    [MaxLength(20)]
    public string? Priority { get; set; }

    public DateOnly? EstimatedEndDate { get; set; }

    public List<string>? Tags { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}
