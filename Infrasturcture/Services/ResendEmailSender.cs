using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public ResendEmailSender(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task SendConfirmEmailAsync(
        string toEmail,
        string confirmLink,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _cfg["Resend:ApiKey"];
        var from = _cfg["Resend:FromEmail"];

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.resend.com/emails"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            from = from,
            to = toEmail,
            subject = "Confirm your email",
            html = $"<p>Confirm your email:</p><a href=\"{confirmLink}\">{confirmLink}</a>"
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
