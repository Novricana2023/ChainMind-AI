namespace ChainMind.Core.Interfaces;

public interface IAIProvider
{
    string ProviderName { get; }
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
