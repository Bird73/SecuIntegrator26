using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecuIntegrator26.Core.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SecuIntegrator26.Infrastructure.Services
{
    public class PlaywrightBridgeService : IWebScraper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlaywrightBridgeService> _logger;

        public PlaywrightBridgeService(IConfiguration configuration, ILogger<PlaywrightBridgeService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> FetchPageContentAsync(string url, string? waitSelector = null)
        {
            var workerPath = _configuration["Playwright:WorkerPath"];
            if (string.IsNullOrEmpty(workerPath) || !File.Exists(workerPath))
            {
                throw new FileNotFoundException($"Playwright Worker not found at configured path: {workerPath}");
            }

            _logger.LogInformation("Invoking Playwright Worker for URL: {Url}", url);

            var startInfo = new ProcessStartInfo
            {
                FileName = workerPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(url);
            if (!string.IsNullOrEmpty(waitSelector))
            {
                startInfo.ArgumentList.Add(waitSelector);
            }

            using var process = new Process { StartInfo = startInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) => { if (args.Data != null) outputBuilder.AppendLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) errorBuilder.AppendLine(args.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Playwright Worker Failed. ExitCode: {ExitCode}. Error: {Error}", process.ExitCode, errorBuilder);
                throw new Exception($"Playwright Worker failed: {errorBuilder}");
            }

            return outputBuilder.ToString();
        }
    }
}
