namespace ChainMind.Core.Models;

public record GenerateContractRequest(string Prompt);
public record ExplainContractRequest(string ContractCode);
public record AuditContractRequest(string ContractCode);
public record DeploymentRequest(string ContractCode, string Network);
public record GenerateTestsRequest(string ContractCode);
public record GasAnalysisRequest(string ContractCode);
public record AgentModeRequest(string ContractCode, string? AdditionalContext);
public record GenerateDocumentationRequest(string ContractCode);
