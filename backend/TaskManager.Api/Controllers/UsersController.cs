using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Dtos;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetAll() =>
        Ok(await users.GetAllAsync());

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var created = await users.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, created);
    }
}
