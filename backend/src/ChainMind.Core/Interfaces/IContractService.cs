using ChainMind.Core.Models;

namespace ChainMind.Core.Interfaces;

public interface IContractService
{
    Task<GenerateContractResponse> GenerateContractAsync(GenerateContractRequest request, CancellationToken cancellationToken = default);
    Task<ExplainContractResponse> ExplainContractAsync(ExplainContractRequest request, CancellationToken cancellationToken = default);
    Task<AuditContractResponse> AuditContractAsync(AuditContractRequest request, CancellationToken cancellationToken = default);
    Task<DeploymentResponse> GenerateDeploymentAsync(DeploymentRequest request, CancellationToken cancellationToken = default);
    Task<GenerateTestsResponse> GenerateTestsAsync(GenerateTestsRequest request, CancellationToken cancellationToken = default);
    Task<GasAnalysisResponse> AnalyzeGasAsync(GasAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<AgentModeResponse> RunAgentModeAsync(AgentModeRequest request, CancellationToken cancellationToken = default);
    Task<GenerateDocumentationResponse> GenerateDocumentationAsync(GenerateDocumentationRequest request, CancellationToken cancellationToken = default);
}
