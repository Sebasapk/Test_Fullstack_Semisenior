using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.Dtos;

public class CreateUserRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;
}
