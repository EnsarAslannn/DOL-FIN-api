using System.Security.Claims;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly Data.ApplicationDBContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            Data.ApplicationDBContext context
        )
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
                return Unauthorized("Invalid username!");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                loginDto.Password,
                false
            );

            if (!result.Succeeded)
                return Unauthorized("Username not found and/or password incorrect");

            SetAuthCookie(await _tokenService.CreateToken(user));

            return Ok(
                new NewUserDto
                {
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    WalletBalance = user.WalletBalance
                }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var appUser = new AppUser
            {
                UserName = registerDto.Username!,
                Email = registerDto.Email!,
                WalletBalance = 0
            };

            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password!);

            if (!createdUser.Succeeded)
                return BadRequest(createdUser.Errors);

            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");

            if (!roleResult.Succeeded)
                return BadRequest(roleResult.Errors);

            SetAuthCookie(await _tokenService.CreateToken(appUser));

            return Ok(
                new NewUserDto
                {
                    UserName = appUser.UserName ?? string.Empty,
                    Email = appUser.Email ?? string.Empty,
                    WalletBalance = appUser.WalletBalance
                }
            );
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Ok();
        }

        private void SetAuthCookie(string token)
        {
            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                }
            );
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userName = User.Identity?.Name
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var userById = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (userById != null)
                    {
                        return Ok(new { userById.WalletBalance, userById.UserName, userById.Email });
                    }
                }
                return Unauthorized("User identity context could not be resolved from token claims.");
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound("User database record not found.");

            return Ok(new { user.WalletBalance, user.UserName, user.Email });
        }
    }
}