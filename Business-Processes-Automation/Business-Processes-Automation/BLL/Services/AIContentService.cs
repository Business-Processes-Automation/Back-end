using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Settings;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Business_Processes_Automation.BLL.Services
{
    public class AIContentService : IAIContentService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AIContentService(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            IConfiguration configuration, AppDbContext dbContext)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _configuration = configuration;
            _dbContext = dbContext;

        }

        public async Task<string> GeneratePostTextAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text =
                                $"Напиши пост для Facebook на тему: {prompt}"
                        }
                    }
                }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_settings.ApiKey}";

            var response = await _httpClient.PostAsync(
                url,
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseJson =
                await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(responseJson);

            var generatedText =
                document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

            return generatedText ?? string.Empty;
        }

        public async Task<string> GenerateAndSaveAsync(
    int postId,
    string prompt,
    CancellationToken cancellationToken = default)
        {
            var generatedText = await GeneratePostTextAsync(
                prompt,
                cancellationToken);

            var generation = new AIContentGeneration
            {
                PostId = postId,
                Prompt = prompt,
                GeneratedText = generatedText
            };

            _dbContext.AiContentGenerations.Add(generation);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return generatedText;
        }

    }
}
