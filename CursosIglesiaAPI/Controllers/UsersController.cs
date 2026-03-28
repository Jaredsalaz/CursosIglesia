using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/user")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfile>> GetProfile()
    {
        Console.WriteLine("[UsersController] GetProfile called");
        Console.WriteLine($"[UsersController] User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"[UsersController] User claims: {User.Claims.Count()}");
        
        try
        {
            var profile = await _userService.GetProfileAsync();
            return Ok(profile);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[UsersController] UnauthorizedAccessException: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UsersController] Exception: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var response = await _userService.UpdateProfileAsync(request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("password")]
    public async Task<ActionResult<ProfileResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var response = await _userService.UpdatePasswordAsync(request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<List<PaymentMethod>>> GetPaymentMethods()
    {
        var methods = await _userService.GetPaymentMethodsAsync();
        return Ok(methods);
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult> AddPaymentMethod([FromBody] PaymentMethod method)
    {
        await _userService.AddPaymentMethodAsync(method);
        return Ok();
    }

    [HttpDelete("payment-methods/{id}")]
    public async Task<ActionResult> RemovePaymentMethod(Guid id)
    {
        await _userService.RemovePaymentMethodAsync(id);
        return Ok();
    }

    [HttpPut("notifications")]
    public async Task<ActionResult> UpdateNotifications([FromBody] NotificationPreferences preferences)
    {
        await _userService.UpdateNotificationsAsync(preferences);
        return Ok();
    }
}
