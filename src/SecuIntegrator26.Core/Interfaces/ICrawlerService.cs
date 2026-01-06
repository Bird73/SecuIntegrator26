using SecuIntegrator26.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface ICrawlerService
    {
        /// <summary>
        /// 從證交所/櫃買中心抓取股票清單
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<StockSymbolDto>> FetchStockSymbolsAsync();
    }
}
