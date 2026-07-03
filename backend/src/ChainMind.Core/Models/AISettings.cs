namespace ChainMind.Core.Models;

public class AISettings
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "ChainMind";
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OpenAIBaseUrl { get; set; } = "https://api.openai.com/v1";
    public string OpenAIModel { get; set; } = "gpt-4o-mini";
    public string ClaudeApiKey { get; set; } = string.Empty;
    public string ClaudeBaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public string ClaudeModel { get; set; } = "claude-sonnet-4-20250514";
}
