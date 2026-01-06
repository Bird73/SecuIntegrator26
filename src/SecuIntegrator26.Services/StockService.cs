using SecuIntegrator26.Core.Entities;
using SecuIntegrator26.Core.Interfaces;
using SecuIntegrator26.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuIntegrator26.Services
{
    public class StockService : IStockService
    {
        private readonly IRepository<StockSymbol> _stockRepo;
        private readonly IRepository<DailyClosingQuote> _quoteRepo;
        private readonly IRepository<MonthlyRevenue> _revenueRepo;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IRepository<StockSymbol> stockRepo,
            IRepository<DailyClosingQuote> quoteRepo,
            IRepository<MonthlyRevenue> revenueRepo,
            ILogger<StockService> logger)
        {
            _stockRepo = stockRepo;
            _quoteRepo = quoteRepo;
            _revenueRepo = revenueRepo;
            _logger = logger;
        }

        public async Task<IReadOnlyList<StockSymbolDto>> GetStockSymbolsAsync(bool activeOnly = true)
        {
            var list = await _stockRepo.ListAllAsync();
            if (activeOnly)
            {
                list = list.Where(x => x.IsActive).ToList();
            }

            return list.Select(x => new StockSymbolDto
            {
                StockCode = x.StockCode,
                Name = x.Name,
                MarketType = x.MarketType,
                Industry = x.Industry,
                IsActive = x.IsActive
            }).ToList();
        }

        public async Task<StockSymbolDto?> GetStockSymbolAsync(string code)
        {
            var entity = await _stockRepo.GetByIdAsync(code);
            if (entity == null) return null;

            return new StockSymbolDto
            {
                StockCode = entity.StockCode,
                Name = entity.Name,
                MarketType = entity.MarketType,
                Industry = entity.Industry,
                IsActive = entity.IsActive
            };
        }

        public async Task AddOrUpdateStockSymbolAsync(StockSymbolDto dto)
        {
            var existing = await _stockRepo.GetByIdAsync(dto.StockCode);
            if (existing == null)
            {
                await _stockRepo.AddAsync(new StockSymbol
                {
                    StockCode = dto.StockCode,
                    Name = dto.Name,
                    MarketType = dto.MarketType,
                    Industry = dto.Industry,
                    IsActive = dto.IsActive
                });
            }
            else
            {
                existing.Name = dto.Name;
                existing.MarketType = dto.MarketType;
                existing.Industry = dto.Industry;
                existing.IsActive = dto.IsActive;
                await _stockRepo.UpdateAsync(existing);
            }
        }

        // 注意：這裡先用簡單的 ListAll 過濾，實際應用應在 Repository 層增加 Specification Pattern 或 Query 方法以優化效能
        public async Task<IReadOnlyList<DailyClosingQuoteDto>> GetDailyQuotesAsync(string stockCode, DateTime startDate, DateTime endDate)
        {
            var allQuotes = await _quoteRepo.ListAllAsync();
            var filtered = allQuotes
                .Where(q => q.StockCode == stockCode && q.TradeDate >= startDate && q.TradeDate <= endDate)
                .OrderBy(q => q.TradeDate)
                .Select(q => new DailyClosingQuoteDto
                {
                    StockCode = q.StockCode,
                    TradeDate = q.TradeDate,
                    ClosingPrice = q.ClosingPrice,
                    Change = q.Change,
                    HighestPrice = q.HighestPrice,
                    LowestPrice = q.LowestPrice,
                    OpeningPrice = q.OpeningPrice,
                    TransactionCount = q.TransactionCount,
                    TradeValue = q.TradeValue,
                    TradeVolume = q.TradeVolume
                }).ToList();
            
            return filtered;
        }

        public async Task SaveDailyQuoteAsync(DailyClosingQuoteDto dto)
        {
            var existing = await _quoteRepo.GetByIdAsync(dto.StockCode, dto.TradeDate);
            if (existing == null)
            {
                await _quoteRepo.AddAsync(new DailyClosingQuote
                {
                    StockCode = dto.StockCode,
                    TradeDate = dto.TradeDate,
                    OpeningPrice = dto.OpeningPrice,
                    HighestPrice = dto.HighestPrice,
                    LowestPrice = dto.LowestPrice,
                    ClosingPrice = dto.ClosingPrice,
                    Change = dto.Change,
                    TransactionCount = dto.TransactionCount,
                    TradeValue = dto.TradeValue,
                    TradeVolume = dto.TradeVolume
                });
            }
            else
            {
                 // Update specific fields if needed
                 existing.ClosingPrice = dto.ClosingPrice;
                 existing.Change = dto.Change;
                 existing.TradeVolume = dto.TradeVolume;
                 await _quoteRepo.UpdateAsync(existing);
            }
        }

        public async Task SaveMonthlyRevenueAsync(MonthlyRevenueDto dto)
        {
             var existing = await _revenueRepo.GetByIdAsync(dto.StockCode, dto.YearMonth);
             if (existing == null)
             {
                 await _revenueRepo.AddAsync(new MonthlyRevenue
                 {
                     StockCode = dto.StockCode,
                     YearMonth = dto.YearMonth,
                     RevenueCurrent = dto.RevenueCurrent,
                     RevenueLastYear = dto.RevenueLastYear,
                     MomChange = dto.MomChange,
                     YoyChange = dto.YoyChange
                 });
             }
             else
             {
                 existing.RevenueCurrent = dto.RevenueCurrent;
                 await _revenueRepo.UpdateAsync(existing);
             }
        }
    }
}
