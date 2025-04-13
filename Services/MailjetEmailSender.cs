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
    public class MailjetEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public MailjetEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var client = new HttpClient();

            var apiKey = _config["Mailjet:ApiKey"];
            var secretKey = _config["Mailjet:ApiSecret"];
            var fromEmail = _config["Mailjet:FromEmail"];
            var fromName = _config["Mailjet:FromName"];

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

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send")
            {
                Content = new StringContent(JObject.FromObject(payload).ToString(), Encoding.UTF8, "application/json")
            };

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception($"Mailjet send failed: {response.StatusCode}\n{result}");
            }
        }
    }
}
