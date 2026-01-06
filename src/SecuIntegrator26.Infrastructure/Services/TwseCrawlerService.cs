using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SecuIntegrator26.Core.Interfaces;
using SecuIntegrator26.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SecuIntegrator26.Infrastructure.Services
{
    public class TwseCrawlerService : ICrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TwseCrawlerService> _logger;

        public TwseCrawlerService(HttpClient httpClient, ILogger<TwseCrawlerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<StockSymbolDto>> FetchStockSymbolsAsync()
        {
            var result = new List<StockSymbolDto>();

            // 1. 上市 (Mode=2)
            result.AddRange(await FetchIsinAsync("https://isin.twse.com.tw/isin/C_public.jsp?strMode=2", "Listed"));
            
            // 2. 上櫃 (Mode=4)
            result.AddRange(await FetchIsinAsync("https://isin.twse.com.tw/isin/C_public.jsp?strMode=4", "OTC"));
            
            // 3. 興櫃 (Mode=5)
            result.AddRange(await FetchIsinAsync("https://isin.twse.com.tw/isin/C_public.jsp?strMode=5", "Emerging"));

            return result;
        }

        private async Task<List<StockSymbolDto>> FetchIsinAsync(string url, string marketType)
        {
            var list = new List<StockSymbolDto>();
            try
            {
                // ISIN page usually encoded in Big5, but HttpClient might default to UTF-8
                // We fetch bytes and decode correctly
                var responseBytes = await _httpClient.GetByteArrayAsync(url);
                
                // Using a trick: try default encoding first, or Big5 if needed. 
                // .NET Core might need CodePagesEncodingProvider for Big5
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var content = Encoding.GetEncoding("Big5").GetString(responseBytes);

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Parser logic: The table structure of ISIN page
                var rows = doc.DocumentNode.SelectNodes("//tr");
                if (rows == null) return list;

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null || cells.Count < 4) continue;

                    // Format: "1101　台泥"
                    var codeAndName = cells[0].InnerText.Split('　'); // full-width space
                    if (codeAndName.Length < 2) continue;

                    var code = codeAndName[0].Trim();
                    var name = codeAndName[1].Trim();
                    var industry = cells[4].InnerText.Trim();

                    // Filter: Only standard stock codes (4 digits) usually
                    // But we might want ETFs etc. For now, let's take everything that looks like a stock code
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    list.Add(new StockSymbolDto
                    {
                        StockCode = code,
                        Name = name,
                        MarketType = marketType,
                        Industry = industry,
                        IsActive = true
                    });
                }
                
                _logger.LogInformation("Fetched {Count} symbols for {MarketType}", list.Count, marketType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock symbols for {MarketType}", marketType);
            }
            return list;
        }
    }
}
