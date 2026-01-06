using SecuIntegrator26.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface IStockService
    {
        Task<IReadOnlyList<StockSymbolDto>> GetStockSymbolsAsync(bool activeOnly = true);
        Task<StockSymbolDto?> GetStockSymbolAsync(string code);
        Task AddOrUpdateStockSymbolAsync(StockSymbolDto dto);
        
        Task<IReadOnlyList<DailyClosingQuoteDto>> GetDailyQuotesAsync(string stockCode, DateTime startDate, DateTime endDate);
        
        // 用於排程爬蟲寫入資料
        Task SaveDailyQuoteAsync(DailyClosingQuoteDto dto);
        Task SaveMonthlyRevenueAsync(MonthlyRevenueDto dto);
    }
}
