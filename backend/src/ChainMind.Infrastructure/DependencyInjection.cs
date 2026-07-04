using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;
using ChainMind.Infrastructure.AI;
using ChainMind.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChainMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<AISettings>(configuration.GetSection(AISettings.SectionName));
        services.AddSingleton<IAnalyticsRepository, InMemoryAnalyticsRepository>();

        var aiSettings = configuration.GetSection(AISettings.SectionName).Get<AISettings>() ?? new AISettings();
        var provider = aiSettings.Provider?.ToLowerInvariant() ?? "chainmind";

        switch (provider)
        {
            case "openai" when !string.IsNullOrEmpty(aiSettings.OpenAIApiKey):
                services.AddHttpClient<OpenAIProvider>(client =>
                {
                    client.BaseAddress = new Uri(aiSettings.OpenAIBaseUrl.TrimEnd('/') + "/");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiSettings.OpenAIApiKey}");
                    client.Timeout = TimeSpan.FromMinutes(3);
                });
                services.AddSingleton<IAIProvider>(sp =>
                    new CachingAIProvider(sp.GetRequiredService<OpenAIProvider>(), sp.GetRequiredService<IMemoryCache>()));
                break;
            case "claude" when !string.IsNullOrEmpty(aiSettings.ClaudeApiKey):
                services.AddHttpClient<ClaudeProvider>(client =>
                {
                    client.BaseAddress = new Uri(aiSettings.ClaudeBaseUrl.TrimEnd('/') + "/");
                    client.DefaultRequestHeaders.Add("x-api-key", aiSettings.ClaudeApiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    client.Timeout = TimeSpan.FromMinutes(3);
                });
                services.AddSingleton<IAIProvider>(sp =>
                    new CachingAIProvider(sp.GetRequiredService<ClaudeProvider>(), sp.GetRequiredService<IMemoryCache>()));
                break;
            default:
                services.AddSingleton<IAIProvider>(sp =>
                    new CachingAIProvider(new ChainMindEngineProvider(), sp.GetRequiredService<IMemoryCache>()));
                break;
        }

        return services;
    }
}
