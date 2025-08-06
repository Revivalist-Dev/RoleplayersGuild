using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RoleplayersGuild.Project.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json; // 1. Using System.Text.Json
using System.Text.Json.Serialization; // For JsonPropertyName attribute

namespace RoleplayersGuild.Site.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        // 2. Private record to strongly-type the Google API response
        private sealed record RecaptchaApiResponse(
            [property: JsonPropertyName("success")] bool Success,
            [property: JsonPropertyName("score")] double Score,
            [property: JsonPropertyName("action")] string Action,
            [property: JsonPropertyName("hostname")] string Hostname,
            [property: JsonPropertyName("error-codes")] List<string>? ErrorCodes
        );

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RecaptchaSettings _settings;
        private readonly ILogger<ReCaptchaService> _logger;

        public ReCaptchaService(IHttpClientFactory httpClientFactory, IOptions<RecaptchaSettings> settings, ILogger<ReCaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;

            if (string.IsNullOrEmpty(_settings.SecretKey))
            {
                throw new InvalidOperationException("reCAPTCHA SecretKey is not configured.");
            }
        }

        public async Task<ReCaptchaResult> ValidateAsync(string recaptchaToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _settings.SecretKey),
                    new KeyValuePair<string, string>("response", recaptchaToken)
                });

                var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                // 3. Deserialize directly into the record instead of parsing manually
                var apiResponse = JsonSerializer.Deserialize<RecaptchaApiResponse>(jsonString);

                if (apiResponse is null)
                {
                    _logger.LogWarning("Failed to deserialize reCAPTCHA response.");
                    return new ReCaptchaResult(false, errorCodes: new[] { "recaptcha-deserialization-failed" });
                }

                return new ReCaptchaResult(
                    apiResponse.Success,
                    apiResponse.Score,
                    apiResponse.Action ?? string.Empty,
                    apiResponse.Hostname ?? string.Empty,
                    apiResponse.ErrorCodes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during reCAPTCHA validation.");
                return new ReCaptchaResult(false, errorCodes: new[] { "recaptcha-exception" });
            }
        }
    }
}