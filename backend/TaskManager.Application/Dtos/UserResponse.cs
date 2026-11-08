namespace TaskManager.Application.Dtos;

public record UserResponse(int Id, string Name, string Email, DateTime CreatedAt);
