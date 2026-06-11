using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Handles user registration and authentication.
/// These are the only public endpoints in the API: no JWT required.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">User registration data</param>
    /// <response code="201">User registered successfully</response>
    /// <response code="409">Email already registered</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync([FromBody] CreateUserDto dto)
    {
        var user = await _userService.RegisterAsync(dto);
        return Created(string.Empty, user);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <response code="200">Authentication successful, token returned</response>
    /// <response code="401">Invalid email or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        var token = await _userService.LoginAsync(dto);
        return Ok(new { token });
    }
}