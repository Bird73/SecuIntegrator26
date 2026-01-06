using Microsoft.Extensions.Logging;
using Quartz;
using SecuIntegrator26.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace SecuIntegrator26.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncStockSymbolsJob : IJob
    {
        private readonly ICrawlerService _crawlerService;
        private readonly IStockService _stockService;
        private readonly ILogger<SyncStockSymbolsJob> _logger;

        public SyncStockSymbolsJob(
            ICrawlerService crawlerService, 
            IStockService stockService,
            ILogger<SyncStockSymbolsJob> logger)
        {
            _crawlerService = crawlerService;
            _stockService = stockService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting Stock Symbol Sync Job at {Time}", DateTime.Now);

            try
            {
                var symbols = await _crawlerService.FetchStockSymbolsAsync();
                
                _logger.LogInformation("Fetched {Count} symbols. Updating database...", symbols.Count);

                int updatedCount = 0;
                foreach (var symbol in symbols)
                {
                    // Only syncing basic info, logic can be refined
                    await _stockService.AddOrUpdateStockSymbolAsync(symbol);
                    updatedCount++;
                }

                _logger.LogInformation("Stock Symbol Sync Completed. Processed {Count} items.", updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync stock symbols.");
                // Optionally re-throw to let Quartz handle retries
            }
        }
    }
}
