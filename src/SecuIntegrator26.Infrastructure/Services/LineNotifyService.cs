using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecuIntegrator26.Core.Interfaces;

namespace SecuIntegrator26.Infrastructure.Services
{
    public class LineNotifyService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LineNotifyService> _logger;
        private const string LINE_NOTIFY_API_URL = "https://notify-api.line.me/api/notify";

        public LineNotifyService(HttpClient httpClient, IConfiguration configuration, ILogger<LineNotifyService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessageAsync(string message)
        {
            var token = _configuration["LineNotify:Token"];
            if (string.IsNullOrEmpty(token) || token == "YOUR_LINE_NOTIFY_TOKEN_HERE")
            {
                _logger.LogWarning("Line Notify Token is not configured. Message skipped: {Message}", message);
                return;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, LINE_NOTIFY_API_URL);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "message", message }
                });

                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send Line Notify. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Line Notify message.");
            }
        }
    }
}
