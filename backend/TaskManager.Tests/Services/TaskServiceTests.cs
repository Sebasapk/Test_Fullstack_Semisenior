using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Tests.Fakes;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly FakeTaskRepository _tasks = new();
    private readonly FakeUserRepository _users = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _users.Items.Add(new User { Id = 1, Name = "Ana", Email = "ana@test.com" });
        _service = new TaskService(_tasks, _users, NullLogger<TaskService>.Instance);
    }

    private static JsonElement Json(string raw) => JsonDocument.Parse(raw).RootElement.Clone();

    private TaskItem SeedTask(TaskStatus status)
    {
        var task = new TaskItem { Id = 10, Title = "Seed", UserId = 1, User = _users.Items[0], Status = status };
        _tasks.Items.Add(task);
        return task;
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPendingTask()
    {
        var result = await _service.CreateAsync(new CreateTaskRequest { Title = "  Write tests  ", UserId = 1 });

        Assert.Equal("Write tests", result.Title);
        Assert.Equal(TaskStatus.Pending, result.Status);
        Assert.Equal("Ana", result.UserName);
        Assert.Single(_tasks.Items);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_BlankTitle_ThrowsBusinessRule(string title)
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CreateAsync(new CreateTaskRequest { Title = title, UserId = 1 }));
    }

    [Fact]
    public async Task CreateAsync_UnknownUser_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(new CreateTaskRequest { Title = "Task", UserId = 99 }));
    }

    [Fact]
    public async Task CreateAsync_AdditionalDataObject_IsStoredAndReturned()
    {
        var result = await _service.CreateAsync(new CreateTaskRequest
        {
            Title = "Task",
            UserId = 1,
            AdditionalData = Json("{\"priority\":\"High\"}")
        });

        Assert.Equal("{\"priority\":\"High\"}", _tasks.Items[0].AdditionalData);
        Assert.Equal("High", result.AdditionalData!.Value.GetProperty("priority").GetString());
    }

    [Fact]
    public async Task CreateAsync_AdditionalDataNotAnObject_ThrowsBusinessRule()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.CreateAsync(new CreateTaskRequest { Title = "Task", UserId = 1, AdditionalData = Json("[1,2]") }));
    }

    [Fact]
    public async Task UpdateStatusAsync_PendingToDone_ThrowsBusinessRule()
    {
        SeedTask(TaskStatus.Pending);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.UpdateStatusAsync(10, new UpdateTaskStatusRequest { Status = TaskStatus.Done }));
        Assert.Equal(0, _tasks.UpdateCalls);
    }

    [Theory]
    [InlineData(TaskStatus.Pending, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Done)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Pending)]
    [InlineData(TaskStatus.Done, TaskStatus.InProgress)]
    public async Task UpdateStatusAsync_AllowedTransition_UpdatesStatusAndTimestamp(TaskStatus from, TaskStatus to)
    {
        var task = SeedTask(from);
        var before = DateTime.UtcNow;

        var result = await _service.UpdateStatusAsync(10, new UpdateTaskStatusRequest { Status = to });

        Assert.Equal(to, result.Status);
        Assert.True(task.UpdatedAt >= before);
        Assert.Equal(1, _tasks.UpdateCalls);
    }

    [Fact]
    public async Task UpdateStatusAsync_UnknownTask_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateStatusAsync(99, new UpdateTaskStatusRequest { Status = TaskStatus.InProgress }));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatusAndUser()
    {
        SeedTask(TaskStatus.Pending);
        _tasks.Items.Add(new TaskItem { Id = 11, Title = "Other", UserId = 1, User = _users.Items[0], Status = TaskStatus.Done });

        var result = await _service.GetAllAsync(TaskStatus.Done, 1, null);

        Assert.Single(result);
        Assert.Equal(11, result[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_InvalidStoredJson_ReturnsNullAdditionalData()
    {
        var task = SeedTask(TaskStatus.Pending);
        task.AdditionalData = "{not json";

        var result = await _service.GetAllAsync(null, null, null);

        Assert.Null(result[0].AdditionalData);
    }
}
