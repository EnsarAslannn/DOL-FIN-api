using api.Dtos;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Service
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepo;
        private readonly IStockRepository _stockRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public PortfolioService(
            IPortfolioRepository portfolioRepo,
            IStockRepository stockRepo,
            ITransactionRepository transactionRepo,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager
        )
        {
            _portfolioRepo = portfolioRepo;
            _stockRepo = stockRepo;
            _transactionRepo = transactionRepo;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<PortfolioDto>> GetUserPortfolioAsync(AppUser user)
        {
            return await _portfolioRepo.GetUserPortfolio(user);
        }

        public async Task<object> BuyStockAsync(AppUser user, string symbol, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            var stock = await _stockRepo.GetBySymbolAsync(symbol);
            if (stock == null)
                throw new InvalidOperationException("Stock not found");

            decimal totalCost = stock.Purchase * quantity;

            if (user.WalletBalance < totalCost)
                throw new InvalidOperationException($"Insufficient funds. Required: ${totalCost:F2}, Available: ${user.WalletBalance:F2}");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.WalletBalance -= totalCost;
                await _userManager.UpdateAsync(user);

                var existingPosition = await _portfolioRepo.GetByAppUserAndStockId(user.Id, stock.Id);

                if (existingPosition != null)
                {
                    decimal existingTotalCost = existingPosition.AveragePrice * existingPosition.Quantity;
                    decimal newTotalCost = existingTotalCost + totalCost;

                    existingPosition.Quantity += quantity;
                    existingPosition.AveragePrice = newTotalCost / existingPosition.Quantity;

                    await _portfolioRepo.UpdateAsync(existingPosition);
                }
                else
                {
                    var portfolioModel = new Portfolio
                    {
                        StockId = stock.Id,
                        AppUserId = user.Id,
                        Quantity = quantity,
                        AveragePrice = stock.Purchase
                    };
                    await _portfolioRepo.CreateAsync(portfolioModel);
                }

                await _transactionRepo.AddAsync(new Transaction
                {
                    AppUserId = user.Id,
                    Symbol = stock.Symbol.ToUpper(),
                    CompanyName = stock.CompanyName,
                    TransactionType = "BUY",
                    Quantity = quantity,
                    Price = stock.Purchase,
                    Timestamp = DateTime.UtcNow
                });

                return (object)new { Message = "Stock purchased successfully", NewBalance = user.WalletBalance };
            });
        }

        public async Task<object> SellStockAsync(AppUser user, string symbol, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            var stock = await _stockRepo.GetBySymbolAsync(symbol);
            if (stock == null)
                throw new InvalidOperationException("Stock not found");

            var existingPosition = await _portfolioRepo.GetByAppUserAndStockId(user.Id, stock.Id);
            if (existingPosition == null || existingPosition.Quantity < quantity)
                throw new InvalidOperationException("Insufficient stock quantity in portfolio to execute this sale");

            decimal totalRevenue = stock.Purchase * quantity;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.WalletBalance += totalRevenue;
                await _userManager.UpdateAsync(user);

                if (existingPosition.Quantity == quantity)
                {
                    await _portfolioRepo.DeletePortfolio(user, symbol);
                }
                else
                {
                    existingPosition.Quantity -= quantity;
                    await _portfolioRepo.UpdateAsync(existingPosition);
                }

                await _transactionRepo.AddAsync(new Transaction
                {
                    AppUserId = user.Id,
                    Symbol = stock.Symbol.ToUpper(),
                    CompanyName = stock.CompanyName,
                    TransactionType = "SELL",
                    Quantity = quantity,
                    Price = stock.Purchase,
                    Timestamp = DateTime.UtcNow
                });

                return (object)new { Message = "Stock sold successfully", NewBalance = user.WalletBalance };
            });
        }

        public async Task<object> DepositFundsAsync(AppUser user, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be greater than 0");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.WalletBalance += amount;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    throw new Exception("Failed to update user wallet balance.");

                await _transactionRepo.AddAsync(new Transaction
                {
                    AppUserId = user.Id,
                    Symbol = "CASH",
                    CompanyName = "Wallet Deposit",
                    TransactionType = "DEPOSIT",
                    Quantity = 1,
                    Price = amount,
                    Timestamp = DateTime.UtcNow
                });

                return (object)new { Message = "Funds deposited successfully", NewBalance = user.WalletBalance };
            });
        }

        public async Task<object> WithdrawFundsAsync(AppUser user, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdraw amount must be greater than 0");

            if (user.WalletBalance < amount)
                throw new InvalidOperationException($"Insufficient funds. Available: ${user.WalletBalance:F2}");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.WalletBalance -= amount;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    throw new Exception("Failed to update user wallet balance.");

                await _transactionRepo.AddAsync(new Transaction
                {
                    AppUserId = user.Id,
                    Symbol = "CASH",
                    CompanyName = "Wallet Withdraw",
                    TransactionType = "WITHDRAW",
                    Quantity = 1,
                    Price = amount,
                    Timestamp = DateTime.UtcNow
                });

                return (object)new { Message = "Funds withdrawn successfully", NewBalance = user.WalletBalance };
            });
        }
    }
}
