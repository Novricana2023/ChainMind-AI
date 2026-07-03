namespace ChainMind.Core.Models;

public record GenerateContractResponse(string ContractCode, string ContractName, string Language = "solidity");
public record ExplainContractResponse(string Summary, string BusinessPurpose, string BeginnerExplanation, List<string> SecurityConsiderations);
public record AuditFinding(string Severity, string Category, string Title, string Description, string Recommendation);
public record AuditContractResponse(List<AuditFinding> Findings, int SecurityScore, string OverallAssessment);
public record DeploymentStep(string Title, string Description, string Command);
public record DeploymentResponse(string Network, List<DeploymentStep> Steps, string VerificationGuide);
public record GenerateTestsResponse(string TestCode, string Framework = "hardhat");
public record GasOptimization(string Title, string Description, string EstimatedSavings);
public record GasAnalysisResponse(int EfficiencyScore, string EstimatedGasUsage, List<GasOptimization> Optimizations, string Summary);
public record AgentRecommendation(string Priority, string Category, string Recommendation, string Reasoning);
public record AgentModeResponse(string ExecutiveSummary, List<AgentRecommendation> Recommendations, string DetailedReport, int RiskScore);
public record GenerateDocumentationResponse(string Markdown, string Title);
public record DashboardStats(int ContractsGenerated, int AuditsCompleted, double SecurityScoreAverage, int ActiveSessions);
public record ProjectItem(string Id, string Name, string Type, string CreatedAt, string Status);
public record TemplateItem(string Id, string Name, string Description, string Category);
public record SessionItem(string Id, string Title, string Type, string CreatedAt);
