using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class UserService(IUserRepository users) : IUserService
{
    public async Task<List<UserResponse>> GetAllAsync()
    {
        var all = await users.GetAllAsync();
        return all.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim();

        if (await users.EmailExistsAsync(email))
            throw new ConflictException($"A user with email '{email}' already exists.");

        var user = new User { Name = request.Name.Trim(), Email = email };
        await users.AddAsync(user);

        return ToResponse(user);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.CreatedAt);
}
