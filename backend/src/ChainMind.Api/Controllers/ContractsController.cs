using ChainMind.Application.Services;
using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChainMind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateContractResponse>> Generate(
        [FromBody] GenerateContractRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { error = "Prompt is required." });

        var result = await _contractService.GenerateContractAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("explain")]
    public async Task<ActionResult<ExplainContractResponse>> Explain(
        [FromBody] ExplainContractRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.ExplainContractAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("audit")]
    public async Task<ActionResult<AuditContractResponse>> Audit(
        [FromBody] AuditContractRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.AuditContractAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("deploy")]
    public async Task<ActionResult<DeploymentResponse>> Deploy(
        [FromBody] DeploymentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.GenerateDeploymentAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("tests")]
    public async Task<ActionResult<GenerateTestsResponse>> Tests(
        [FromBody] GenerateTestsRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.GenerateTestsAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("gas")]
    public async Task<ActionResult<GasAnalysisResponse>> Gas(
        [FromBody] GasAnalysisRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.AnalyzeGasAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("agent")]
    public async Task<ActionResult<AgentModeResponse>> Agent(
        [FromBody] AgentModeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.RunAgentModeAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("documentation")]
    public async Task<ActionResult<GenerateDocumentationResponse>> Documentation(
        [FromBody] GenerateDocumentationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContractCode))
            return BadRequest(new { error = "Contract code is required." });

        var result = await _contractService.GenerateDocumentationAsync(request, ct);
        return Ok(result);
    }
}
