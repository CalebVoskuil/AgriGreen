using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AgriGreen.Services
{
    // Implementation of ASP.NET Core's email sender interface using Mailjet API
    public class MailjetEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public MailjetEmailSender(IConfiguration config)
        {
            _config = config;
        }

        // Method to send emails using Mailjet's RESTful API
        // This asynchronously constructs and sends an HTTP request to Mailjet's service
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var client = new HttpClient();

            // Retrieve credentials and sender information from application configuration
            // This avoids hardcoding sensitive values in the source code
            var apiKey = _config["Mailjet:ApiKey"];
            var secretKey = _config["Mailjet:ApiSecret"];
            var fromEmail = _config["Mailjet:FromEmail"];
            var fromName = _config["Mailjet:FromName"];

            // Construct message payload in format expected by Mailjet API
            // This builds a JSON structure with sender, recipient, subject and HTML content
            var payload = new
            {
                Messages = new[]
                {
                    new {
                        From = new { Email = fromEmail, Name = fromName },
                        To = new[] { new { Email = toEmail } },
                        Subject = subject,
                        HTMLPart = htmlMessage
                    }
                }
            };

            // Create HTTP request with appropriate endpoint and content
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send")
            {
                Content = new StringContent(JObject.FromObject(payload).ToString(), Encoding.UTF8, "application/json")
            };

            // Set up HTTP Basic Authentication with API credentials
            // This encodes the username:password pair in Base64 as per HTTP standards
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            // Send request and await response
            var response = await client.SendAsync(request);

            // Check for success and throw exception with details if failed
            // This allows callers to handle email sending failures appropriately
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception($"Mailjet send failed: {response.StatusCode}\n{result}");
            }
        }
    }
}
