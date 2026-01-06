using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface IWebScraper
    {
        /// <summary>
        /// 使用瀏覽器 (Playwright Worker) 抓取網頁內容
        /// </summary>
        /// <param name="url">目標網址</param>
        /// <param name="waitSelector">選用：等待出現的 CSS Selector</param>
        /// <returns>HTML 內容</returns>
        Task<string> FetchPageContentAsync(string url, string? waitSelector = null);
    }
}
