using System.Text.Json;
using System.Text.RegularExpressions;
using ChainMind.Application.Prompts;
using ChainMind.Application.Validation;
using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;

namespace ChainMind.Application.Services;

public class ContractService : IContractService
{
    private readonly IAIProvider _aiProvider;
    private readonly IAnalyticsRepository _analytics;

    public ContractService(IAIProvider aiProvider, IAnalyticsRepository analytics)
    {
        _aiProvider = aiProvider;
        _analytics = analytics;
    }

    public async Task<GenerateContractResponse> GenerateContractAsync(GenerateContractRequest request, CancellationToken cancellationToken = default)
    {
        var userPrompt = AiPrompts.BuildGenerationUserPrompt(request.Prompt);
        var code = CleanCodeBlock(await _aiProvider.CompleteAsync(AiPrompts.GenerateContract, userPrompt, cancellationToken));

        if (_aiProvider.ProviderName != "ChainMind")
        {
            code = await RefineGeneratedCodeAsync(request.Prompt, code, cancellationToken);
        }

        var name = ExtractContractName(code) ?? "GeneratedContract";

        await _analytics.IncrementContractsGeneratedAsync(cancellationToken);
        await _analytics.AddProjectAsync(new ProjectItem(
            Guid.NewGuid().ToString("N")[..8],
            name,
            "Generated",
            DateTime.UtcNow.ToString("O"),
            "Complete"
        ), cancellationToken);

        return new GenerateContractResponse(code, name);
    }

    private async Task<string> RefineGeneratedCodeAsync(string originalPrompt, string code, CancellationToken cancellationToken)
    {
        var staticIssues = ContractValidator.Validate(code);
        var auditResponse = await _aiProvider.CompleteAsync(AiPrompts.AuditContract, code, cancellationToken);
        var audit = ParseJson<AuditJsonDto>(auditResponse);

        var criticalOrHigh = audit.findings?.Any(f =>
            f.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
            f.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)) ?? false;

        var lowScore = audit.securityScore > 0 && audit.securityScore < 85;
        var needsRefine = staticIssues.Count > 0 || criticalOrHigh || lowScore;

        if (!needsRefine)
            return code;

        var auditDtos = audit.findings?.Select(f =>
            new AiPrompts.AuditFindingDto(f.Severity, f.Category, f.Title, f.Description, f.Recommendation))
            ?? Enumerable.Empty<AiPrompts.AuditFindingDto>();

        var fixPrompt = AiPrompts.BuildFixUserPrompt(originalPrompt, code, staticIssues, auditDtos);
        return CleanCodeBlock(await _aiProvider.CompleteAsync(AiPrompts.FixContract, fixPrompt, cancellationToken));
    }

    public async Task<ExplainContractResponse> ExplainContractAsync(ExplainContractRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _aiProvider.CompleteAsync(AiPrompts.ExplainContract, request.ContractCode, cancellationToken);
        var parsed = ParseJson<ExplainJsonDto>(response);
        var result = new ExplainContractResponse(
            parsed.summary ?? "Contract analysis complete.",
            parsed.businessPurpose ?? "Business purpose analysis.",
            parsed.beginnerExplanation ?? "This contract manages blockchain operations.",
            parsed.securityConsiderations ?? new List<string> { "Review access controls", "Validate inputs" }
        );
        await TrackSessionAsync("Contract Explanation", "Explainer", ExtractContractName(request.ContractCode), cancellationToken);
        return result;
    }

    public async Task<AuditContractResponse> AuditContractAsync(AuditContractRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _aiProvider.CompleteAsync(AiPrompts.AuditContract, request.ContractCode, cancellationToken);
        var parsed = ParseJson<AuditJsonDto>(response);
        var result = new AuditContractResponse(
            parsed.findings ?? new List<AuditFinding>(),
            parsed.securityScore > 0 ? parsed.securityScore : 75,
            parsed.overallAssessment ?? "Audit complete."
        );

        await _analytics.IncrementAuditsCompletedAsync(result.SecurityScore, cancellationToken);
        await TrackSessionAsync("Security Audit", "Auditor", ExtractContractName(request.ContractCode), cancellationToken);
        return result;
    }

    public async Task<DeploymentResponse> GenerateDeploymentAsync(DeploymentRequest request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = "You are a Web3 DevOps expert. Generate deployment instructions for " + request.Network + ". " +
            "Include wallet setup, testnet deployment, and verification. Respond in JSON only: " +
            "{\"network\":\"" + request.Network + "\",\"steps\":[{\"title\":\"...\",\"description\":\"...\",\"command\":\"...\"}],\"verificationGuide\":\"...\"}";
        var response = await _aiProvider.CompleteAsync(systemPrompt, request.ContractCode, cancellationToken);
        var parsed = ParseJson<DeploymentJsonDto>(response);
        var result = new DeploymentResponse(
            parsed.network ?? request.Network,
            parsed.steps ?? new List<DeploymentStep>(),
            parsed.verificationGuide ?? "Verify on block explorer."
        );
        await TrackSessionAsync($"{request.Network} Deployment", "Deploy", ExtractContractName(request.ContractCode), cancellationToken);
        return result;
    }

    public async Task<GenerateTestsResponse> GenerateTestsAsync(GenerateTestsRequest request, CancellationToken cancellationToken = default)
    {
        var code = await _aiProvider.CompleteAsync(AiPrompts.GenerateTests, request.ContractCode, cancellationToken);
        await TrackSessionAsync("Test Generation", "Tests", ExtractContractName(request.ContractCode), cancellationToken);
        return new GenerateTestsResponse(CleanCodeBlock(code));
    }

    public async Task<GasAnalysisResponse> AnalyzeGasAsync(GasAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _aiProvider.CompleteAsync(AiPrompts.GasAnalysis, request.ContractCode, cancellationToken);
        var parsed = ParseJson<GasJsonDto>(response);
        var result = new GasAnalysisResponse(
            parsed.efficiencyScore > 0 ? parsed.efficiencyScore : 70,
            parsed.estimatedGasUsage ?? "N/A",
            parsed.optimizations ?? new List<GasOptimization>(),
            parsed.summary ?? "Analysis complete."
        );
        await TrackSessionAsync("Gas Analysis", "Gas", ExtractContractName(request.ContractCode), cancellationToken);
        return result;
    }

    public async Task<AgentModeResponse> RunAgentModeAsync(AgentModeRequest request, CancellationToken cancellationToken = default)
    {
        var userPrompt = request.AdditionalContext != null
            ? $"{request.ContractCode}\n\nAdditional context: {request.AdditionalContext}"
            : request.ContractCode;

        var response = await _aiProvider.CompleteAsync(AiPrompts.AgentMode, userPrompt, cancellationToken);
        var parsed = ParseJson<AgentJsonDto>(response);
        var result = new AgentModeResponse(
            parsed.executiveSummary ?? "Agent review complete.",
            parsed.recommendations ?? new List<AgentRecommendation>(),
            parsed.detailedReport ?? "Detailed analysis provided.",
            parsed.riskScore > 0 ? parsed.riskScore : 50
        );

        await TrackSessionAsync("Agent Review", "Agent", ExtractContractName(request.ContractCode), cancellationToken);

        return result;
    }

    public async Task<GenerateDocumentationResponse> GenerateDocumentationAsync(GenerateDocumentationRequest request, CancellationToken cancellationToken = default)
    {
        var markdown = await _aiProvider.CompleteAsync(AiPrompts.Documentation, request.ContractCode, cancellationToken);
        var title = ExtractContractName(request.ContractCode) ?? "Smart Contract";
        await TrackSessionAsync("Documentation", "Docs", title != "Smart Contract" ? title : null, cancellationToken);
        return new GenerateDocumentationResponse(CleanCodeBlock(markdown), title);
    }

    private async Task TrackSessionAsync(string title, string type, string? contractName, CancellationToken ct)
    {
        var sessionTitle = contractName != null ? $"{title} — {contractName}" : title;
        await _analytics.AddSessionAsync(new SessionItem(
            Guid.NewGuid().ToString("N")[..8],
            sessionTitle,
            type,
            DateTime.UtcNow.ToString("O")
        ), ct);
    }

    private static string CleanCodeBlock(string code)
    {
        code = code.Trim();
        var match = Regex.Match(code, @"```(?:\w+)?\s*([\s\S]*?)```");
        return match.Success ? match.Groups[1].Value.Trim() : code;
    }

    private static string? ExtractContractName(string code)
    {
        var match = Regex.Match(code, @"contract\s+(\w+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static T ParseJson<T>(string response) where T : new()
    {
        try
        {
            var cleaned = CleanJson(response);
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    private static string CleanJson(string response)
    {
        response = response.Trim();
        var match = Regex.Match(response, @"```(?:json)?\s*([\s\S]*?)```");
        if (match.Success) return match.Groups[1].Value.Trim();
        var start = response.IndexOf('{');
        var end = response.LastIndexOf('}');
        return start >= 0 && end > start ? response[start..(end + 1)] : response;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private class ExplainJsonDto
    {
        public string? summary { get; set; }
        public string? businessPurpose { get; set; }
        public string? beginnerExplanation { get; set; }
        public List<string>? securityConsiderations { get; set; }
    }

    private class AuditJsonDto
    {
        public List<AuditFinding>? findings { get; set; }
        public int securityScore { get; set; }
        public string? overallAssessment { get; set; }
    }

    private class DeploymentJsonDto
    {
        public string? network { get; set; }
        public List<DeploymentStep>? steps { get; set; }
        public string? verificationGuide { get; set; }
    }

    private class GasJsonDto
    {
        public int efficiencyScore { get; set; }
        public string? estimatedGasUsage { get; set; }
        public List<GasOptimization>? optimizations { get; set; }
        public string? summary { get; set; }
    }

    private class AgentJsonDto
    {
        public string? executiveSummary { get; set; }
        public List<AgentRecommendation>? recommendations { get; set; }
        public string? detailedReport { get; set; }
        public int riskScore { get; set; }
    }
}
