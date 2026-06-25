using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Handles user registration and authentication.
/// These are the only public endpoints in the API: no JWT required.
/// On successful login, an access token is returned in the response
/// body and a refresh token is set as an httpOnly cookie.
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
    /// Authenticates a user, returning an access token and setting
    /// a refresh token as an httpOnly cookie.
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <response code="200">Authentication successful, access token returned</response>
    /// <response code="401">Invalid email or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        var (loginResponse, refreshToken, expiresAt) = await _userService.LoginAsync(dto);

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh",
            Expires = expiresAt
        });

        return Ok(loginResponse);
    }

    /// <summary>
    /// Generates a new access token using the refresh token cookie.
    /// </summary>
    /// <response code="200">New access token generated</response>
    /// <response code="401">Missing, invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshAsync()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var response = await _userService.RefreshAsync(refreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Logs out the user by invalidating the refresh token.
    /// </summary>
    /// <response code="204">Logged out successfully</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
            await _userService.LogoutAsync(refreshToken);

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/api/auth/refresh"
        });

        return NoContent();
    }
}