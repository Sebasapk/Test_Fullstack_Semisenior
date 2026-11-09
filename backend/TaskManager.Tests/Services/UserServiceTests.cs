using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Tests.Fakes;

namespace TaskManager.Tests.Services;

public class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly UserService _service;

    public UserServiceTests() => _service = new UserService(_users);

    [Fact]
    public async Task CreateAsync_TrimsFieldsAndPersistsUser()
    {
        var result = await _service.CreateAsync(new CreateUserRequest { Name = "  Ana  ", Email = " ana@test.com " });

        Assert.Equal("Ana", result.Name);
        Assert.Equal("ana@test.com", result.Email);
        Assert.Single(_users.Items);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsConflict()
    {
        await _service.CreateAsync(new CreateUserRequest { Name = "Ana", Email = "ana@test.com" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(new CreateUserRequest { Name = "Other", Email = "ANA@test.com" }));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await _service.CreateAsync(new CreateUserRequest { Name = "Ana", Email = "ana@test.com" });
        await _service.CreateAsync(new CreateUserRequest { Name = "Luis", Email = "luis@test.com" });

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }
}
