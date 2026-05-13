using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Auth;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IKeycloakAuthService _authService;

        public AuthController(IKeycloakAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            var validator = new LoginRequestValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));
            try
            {
                var tokens = await _authService.LoginAsync(request.Username, request.Password);
                return Ok(tokens);
            }
            catch (HttpRequestException)
            {
                return Unauthorized("Invalid username or password");
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var tokens = await _authService.RefreshAsync(request.RefreshToken);
                return Ok(tokens);
            }
            catch (HttpRequestException)
            {
                return Unauthorized("Invalid or expired refresh token");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }
    }
}
