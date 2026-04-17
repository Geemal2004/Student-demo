using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.DTOs.Api;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Controllers.Api;

[Route("api/auth")]
public class AuthApiController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe
        });

        if (!result.Success)
        {
            return Unauthorized(new ApiErrorDto { Message = result.Message ?? "Invalid login attempt." });
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            Role = request.Role
        });

        if (!result.Success)
        {
            return BadRequest(new ApiErrorDto { Message = result.Message ?? "Registration failed." });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // DECISION: JWT is stateless. Logout is handled client-side by deleting token.
        return Ok(new { message = "Logged out." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _authService.GetCurrentUserAsync(GetCurrentUserId());
        if (user == null)
        {
            return NotFound(new ApiErrorDto { Message = "User not found." });
        }

        return Ok(user);
    }
}
