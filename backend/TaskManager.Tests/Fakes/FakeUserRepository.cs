using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public Task<List<User>> GetAllAsync() => Task.FromResult(Items.ToList());

    public Task<User?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(Items.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(User user)
    {
        user.Id = Items.Count + 1;
        Items.Add(user);
        return Task.CompletedTask;
    }
}
