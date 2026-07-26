using api.Dtos.Portfolio;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(UserManager<AppUser> userManager, IPortfolioService portfolioService)
        {
            _userManager = userManager;
            _portfolioService = portfolioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPortfolio()
        {
            var appUser = await GetAuthenticatedUserAsync();
            if (appUser == null)
                return Unauthorized("User context not found.");

            var userPortfolio = await _portfolioService.GetUserPortfolioAsync(appUser);
            return Ok(userPortfolio);
        }

        [HttpPost]
        public async Task<IActionResult> AddPortfolio([FromBody] TradeRequestDto request)
        {
            var appUser = await GetAuthenticatedUserAsync();
            if (appUser == null)
                return Unauthorized("User context not found.");

            try
            {
                var result = await _portfolioService.BuyStockAsync(appUser, request.Symbol, request.Quantity);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellPortfolio([FromBody] TradeRequestDto request)
        {
            var appUser = await GetAuthenticatedUserAsync();
            if (appUser == null)
                return Unauthorized("User context not found.");

            try
            {
                var result = await _portfolioService.SellStockAsync(appUser, request.Symbol, request.Quantity);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> DepositFunds([FromBody] AmountRequestDto request)
        {
            var appUser = await GetAuthenticatedUserAsync();
            if (appUser == null)
                return Unauthorized("User context not found.");

            try
            {
                var result = await _portfolioService.DepositFundsAsync(appUser, request.Amount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> WithdrawFunds([FromBody] AmountRequestDto request)
        {
            var appUser = await GetAuthenticatedUserAsync();
            if (appUser == null)
                return Unauthorized("User context not found.");

            try
            {
                var result = await _portfolioService.WithdrawFundsAsync(appUser, request.Amount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<AppUser?> GetAuthenticatedUserAsync()
        {
            var username = User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst("name")?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value
                    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return await _userManager.FindByIdAsync(userId);
                }
                return null;
            }

            return await _userManager.FindByNameAsync(username);
        }
    }
}