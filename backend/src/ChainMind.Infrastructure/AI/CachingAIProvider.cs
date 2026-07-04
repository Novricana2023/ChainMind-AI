using System.Security.Cryptography;
using System.Text;
using ChainMind.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ChainMind.Infrastructure.AI;

public class CachingAIProvider : IAIProvider
{
    private readonly IAIProvider _inner;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public CachingAIProvider(IAIProvider inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public string ProviderName => $"{_inner.ProviderName}+Cache";

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var key = "ai:" + Hash($"{systemPrompt}|{userPrompt}");

        if (_cache.TryGetValue(key, out string? cached) && cached != null)
            return cached;

        var result = await _inner.CompleteAsync(systemPrompt, userPrompt, cancellationToken);
        _cache.Set(key, result, CacheDuration);
        return result;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
