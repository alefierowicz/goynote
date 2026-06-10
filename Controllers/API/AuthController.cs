using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using goynote.Models.System;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(SignInManager<ApplicationUser> signInManager)
        => _signInManager = signInManager;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _signInManager.PasswordSignInAsync(
            req.Email, req.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded) return Ok();
        if (result.IsLockedOut) return StatusCode(429, new { error = "Account locked out." });

        return Unauthorized(new { error = "Invalid credentials." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }
}

public record LoginRequest(string Email, string Password);