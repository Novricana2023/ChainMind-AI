using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;
using ChainMind.Infrastructure.AI;
using ChainMind.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChainMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AISettings>(configuration.GetSection(AISettings.SectionName));
        services.AddSingleton<IAnalyticsRepository, InMemoryAnalyticsRepository>();

        var aiSettings = configuration.GetSection(AISettings.SectionName).Get<AISettings>() ?? new AISettings();
        var provider = aiSettings.Provider?.ToLowerInvariant() ?? "chainmind";

        switch (provider)
        {
            case "openai" when !string.IsNullOrEmpty(aiSettings.OpenAIApiKey):
                services.AddHttpClient<IAIProvider, OpenAIProvider>(client =>
                {
                    client.BaseAddress = new Uri(aiSettings.OpenAIBaseUrl.TrimEnd('/') + "/");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiSettings.OpenAIApiKey}");
                });
                break;
            case "claude" when !string.IsNullOrEmpty(aiSettings.ClaudeApiKey):
                services.AddHttpClient<IAIProvider, ClaudeProvider>(client =>
                {
                    client.BaseAddress = new Uri(aiSettings.ClaudeBaseUrl.TrimEnd('/') + "/");
                    client.DefaultRequestHeaders.Add("x-api-key", aiSettings.ClaudeApiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                });
                break;
            default:
                services.AddSingleton<IAIProvider, ChainMindEngineProvider>();
                break;
        }

        return services;
    }
}
