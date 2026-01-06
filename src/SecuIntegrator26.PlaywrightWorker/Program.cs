using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace SecuIntegrator26.PlaywrightWorker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: SecuIntegrator26.PlaywrightWorker <url> [wait_selector]");
                Environment.Exit(1);
            }

            string url = args[0];
            string? waitSelector = args.Length > 1 ? args[1] : null;

            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();
                
                // Add some headers to look more like a real browser
                await page.SetExtraHTTPHeadersAsync(new System.Collections.Generic.Dictionary<string, string>
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36" }
                });

                await page.GotoAsync(url);

                if (!string.IsNullOrEmpty(waitSelector))
                {
                    await page.WaitForSelectorAsync(waitSelector, new PageWaitForSelectorOptions { Timeout = 30000 });
                }
                else
                {
                    // Default wait strategy
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }

                var content = await page.ContentAsync();
                Console.WriteLine(content);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
