// In F:\...\Site.Services\ReCaptchaService.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RoleplayersGuild.Project.Configuration; // Assuming this is where RecaptchaSettings is
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
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
                var jsonResponse = JObject.Parse(jsonString);

                var isSuccess = jsonResponse.Value<bool>("success");
                var score = jsonResponse.Value<double>("score");
                var action = jsonResponse.Value<string>("action") ?? string.Empty;
                var hostname = jsonResponse.Value<string>("hostname") ?? string.Empty;
                var errorCodes = jsonResponse["error-codes"]?.ToObject<List<string>>();

                return new ReCaptchaResult(isSuccess, score, action, hostname, errorCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during reCAPTCHA validation.");
                return new ReCaptchaResult(false, errorCodes: new[] { "recaptcha-exception" });
            }
        }
    }
}