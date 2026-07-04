using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;
using Microsoft.Extensions.Options;

namespace ChainMind.Infrastructure.AI;

internal static class ContractAnalyzer
{
    internal record ContractProfile(
        string Name,
        bool IsErc20,
        bool IsErc721,
        bool HasOwnable,
        bool HasAccessControl,
        bool HasExternalCalls,
        bool HasDelegateCall,
        bool HasSelfDestruct,
        bool UsesTxOrigin,
        bool HasUncheckedBlocks,
        bool HasReentrancyRisk,
        bool HasMint,
        bool HasBurn,
        bool HasRequireStrings,
        bool HasCustomErrors,
        bool HasEvents,
        int FunctionCount,
        string PragmaVersion
    );

    internal static ContractProfile Analyze(string code)
    {
        var nameMatch = Regex.Match(code, @"contract\s+(\w+)");
        var name = nameMatch.Success ? nameMatch.Groups[1].Value : "Contract";

        return new ContractProfile(
            Name: name,
            IsErc20: code.Contains("ERC20", StringComparison.OrdinalIgnoreCase),
            IsErc721: code.Contains("ERC721", StringComparison.OrdinalIgnoreCase),
            HasOwnable: code.Contains("Ownable", StringComparison.OrdinalIgnoreCase) || code.Contains("onlyOwner"),
            HasAccessControl: code.Contains("AccessControl", StringComparison.OrdinalIgnoreCase),
            HasExternalCalls: Regex.IsMatch(code, @"\.call\{value:") || Regex.IsMatch(code, @"\.transfer\("),
            HasDelegateCall: code.Contains("delegatecall", StringComparison.OrdinalIgnoreCase),
            HasSelfDestruct: code.Contains("selfdestruct", StringComparison.OrdinalIgnoreCase),
            UsesTxOrigin: code.Contains("tx.origin", StringComparison.OrdinalIgnoreCase),
            HasUncheckedBlocks: code.Contains("unchecked", StringComparison.OrdinalIgnoreCase),
            HasReentrancyRisk: Regex.IsMatch(code, @"\.call\{value:") && !code.Contains("nonReentrant"),
            HasMint: Regex.IsMatch(code, @"function\s+mint\s*\(", RegexOptions.IgnoreCase),
            HasBurn: Regex.IsMatch(code, @"function\s+burn\s*\(", RegexOptions.IgnoreCase),
            HasRequireStrings: Regex.IsMatch(code, @"require\s*\([^)]*"""),
            HasCustomErrors: Regex.IsMatch(code, @"error\s+\w+"),
            HasEvents: Regex.IsMatch(code, @"event\s+\w+"),
            FunctionCount: Regex.Matches(code, @"function\s+\w+").Count,
            PragmaVersion: Regex.Match(code, @"pragma\s+solidity\s+([^;]+);").Groups[1].Value.Trim()
        );
    }

    internal static int CalculateSecurityScore(ContractProfile p, int findingCount, int criticalCount, int highCount)
    {
        var score = 100;
        score -= criticalCount * 25;
        score -= highCount * 15;
        score -= findingCount * 3;
        if (!p.HasOwnable && !p.HasAccessControl && p.HasMint) score -= 10;
        if (p.HasReentrancyRisk) score -= 20;
        if (p.UsesTxOrigin) score -= 15;
        if (p.HasDelegateCall) score -= 10;
        if (p.HasSelfDestruct) score -= 10;
        if (p.HasUncheckedBlocks) score -= 8;
        if (p.HasOwnable || p.HasAccessControl) score += 5;
        if (p.HasCustomErrors) score += 3;
        if (p.HasEvents) score += 3;
        return Math.Clamp(score, 0, 100);
    }

    internal static object[] BuildAuditFindings(ContractProfile p)
    {
        var findings = new List<object>();

        if (p.HasReentrancyRisk)
            findings.Add(new { severity = "Critical", category = "Reentrancy", title = "Potential Reentrancy Vulnerability", description = $"External calls detected in {p.Name} without ReentrancyGuard protection.", recommendation = "Apply OpenZeppelin ReentrancyGuard and follow checks-effects-interactions pattern." });

        if (p.UsesTxOrigin)
            findings.Add(new { severity = "High", category = "Access Control", title = "tx.origin Authentication", description = "Contract uses tx.origin for authorization which is vulnerable to phishing attacks.", recommendation = "Replace tx.origin checks with msg.sender." });

        if (p.HasDelegateCall)
            findings.Add(new { severity = "High", category = "Proxy Safety", title = "Delegatecall Usage", description = "Delegatecall detected — improper use can lead to storage collision or unauthorized code execution.", recommendation = "Audit delegatecall targets and use audited proxy patterns (UUPS/Transparent)." });

        if (p.HasSelfDestruct)
            findings.Add(new { severity = "High", category = "Contract Lifecycle", title = "Selfdestruct Present", description = "Contract includes selfdestruct which can permanently destroy the contract and funds.", recommendation = "Restrict selfdestruct to multi-sig or timelock governance." });

        if (p.HasMint && p.HasOwnable && !p.HasAccessControl)
            findings.Add(new { severity = "Medium", category = "Access Control", title = "Centralized Mint Authority", description = $"{p.Name} grants minting privileges to a single owner address.", recommendation = "Use AccessControl with MINTER_ROLE or implement supply caps with timelock." });

        if (p.HasExternalCalls && !p.HasReentrancyRisk)
            findings.Add(new { severity = "Medium", category = "External Calls", title = "External Value Transfers", description = "Contract performs external calls that should be reviewed for safe execution order.", recommendation = "Ensure state updates occur before external calls." });

        if (p.HasUncheckedBlocks)
            findings.Add(new { severity = "Medium", category = "Integer Safety", title = "Unchecked Arithmetic Block", description = "Unchecked blocks bypass Solidity 0.8 overflow protection.", recommendation = "Remove unchecked blocks or document why overflow is impossible." });

        if (!p.HasEvents && (p.HasMint || p.HasBurn))
            findings.Add(new { severity = "Low", category = "Observability", title = "Missing Custom Events", description = "State-changing operations lack dedicated event emissions.", recommendation = "Emit events for mint, burn, and role changes for off-chain indexing." });

        if (p.HasRequireStrings && !p.HasCustomErrors)
            findings.Add(new { severity = "Low", category = "Gas Optimization", title = "String-Based Require Messages", description = "Require statements use string literals increasing deployment and revert gas.", recommendation = "Replace with custom errors: error InsufficientBalance();" });

        if (!p.HasOwnable && !p.HasAccessControl && p.FunctionCount > 3)
            findings.Add(new { severity = "Medium", category = "Access Control", title = "Missing Access Control", description = "No Ownable or AccessControl pattern detected on a multi-function contract.", recommendation = "Add role-based access control for privileged functions." });

        if (findings.Count == 0)
            findings.Add(new { severity = "Info", category = "Best Practices", title = "No Critical Issues Detected", description = $"{p.Name} follows standard patterns. Continue with testnet deployment and formal verification.", recommendation = "Run Slither/Mythril and complete test coverage before mainnet." });

        return findings.ToArray();
    }
}

public class ChainMindEngineProvider : IAIProvider
{
    public string ProviderName => "ChainMind";

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var lower = systemPrompt.ToLowerInvariant();
        string result;

        if (lower.Contains("solidity") && lower.Contains("generate"))
            result = GenerateContract(userPrompt);
        else if (lower.Contains("educator") || lower.Contains("explain"))
            result = ExplainContract(userPrompt);
        else if (lower.Contains("auditor") || lower.Contains("vulnerabilities"))
            result = AuditContract(userPrompt);
        else if (lower.Contains("devops") || lower.Contains("deployment"))
            result = GenerateDeployment(systemPrompt, userPrompt);
        else if (lower.Contains("hardhat") || lower.Contains("testing"))
            result = GenerateTests(userPrompt);
        else if (lower.Contains("gas optimization"))
            result = AnalyzeGas(userPrompt);
        else if (lower.Contains("security engineer"))
            result = GenerateAgentReport(userPrompt);
        else if (lower.Contains("technical writer") || lower.Contains("markdown"))
            result = GenerateDocumentation(userPrompt);
        else
            result = JsonSerializer.Serialize(new { message = "Analysis complete." });

        return Task.FromResult(result);
    }

    private static string GenerateContract(string prompt)
    {
        var tokenMatch = Regex.Match(prompt, @"(?:called|named)\s+(\w+)", RegexOptions.IgnoreCase);
        var supplyMatch = Regex.Match(prompt, @"([\d,]+)\s*(?:tokens|supply)?", RegexOptions.IgnoreCase);
        var nftMatch = Regex.IsMatch(prompt, @"NFT|ERC721|non-fungible", RegexOptions.IgnoreCase);
        var name = tokenMatch.Success ? tokenMatch.Groups[1].Value : "ChainToken";
        var supply = supplyMatch.Success ? supplyMatch.Groups[1].Value.Replace(",", "") : "1000000";
        var symbol = name.Length > 4 ? name[..4].ToUpperInvariant() : name.ToUpperInvariant();

        if (nftMatch)
        {
            return $$"""
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract {{name}} is ERC721, Ownable {
    uint256 private _nextTokenId;
    uint256 public constant MAX_SUPPLY = {{supply}};

    constructor(address initialOwner) ERC721("{{name}}", "{{symbol}}") Ownable(initialOwner) {}

    function safeMint(address to) external onlyOwner {
        require(_nextTokenId < MAX_SUPPLY, "Max supply reached");
        _safeMint(to, _nextTokenId++);
    }

    function totalMinted() external view returns (uint256) {
        return _nextTokenId;
    }
}
""";
        }

        return $$"""
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract {{name}} is ERC20, Ownable {
    uint256 public constant MAX_SUPPLY = {{supply}} * 10 ** decimals();

    constructor(address initialOwner)
        ERC20("{{name}}", "{{symbol}}")
        Ownable(initialOwner)
    {
        _mint(initialOwner, MAX_SUPPLY);
    }

    function burn(uint256 amount) external {
        _burn(msg.sender, amount);
    }

    function mint(address to, uint256 amount) external onlyOwner {
        require(to != address(0), "Zero address");
        require(totalSupply() + amount <= MAX_SUPPLY, "Exceeds max supply");
        _mint(to, amount);
    }
}
""";
    }

    private static string ExplainContract(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var typeDesc = p.IsErc721 ? "ERC721 NFT collection" : p.IsErc20 ? "ERC20 fungible token" : "Solidity smart contract";
        var accessDesc = p.HasAccessControl ? "role-based access control (AccessControl)" :
            p.HasOwnable ? "single-owner access control (Ownable)" : "no explicit access control pattern";

        var security = new List<string>();
        if (p.HasReentrancyRisk) security.Add("Reentrancy risk from external calls — add ReentrancyGuard");
        if (p.UsesTxOrigin) security.Add("Avoid tx.origin for authentication");
        if (p.HasMint && !p.HasAccessControl) security.Add("Review mint function permissions and supply caps");
        if (p.HasExternalCalls) security.Add("Validate external call ordering (checks-effects-interactions)");
        if (p.HasUncheckedBlocks) security.Add("Review unchecked arithmetic blocks");
        security.Add($"Pragma {p.PragmaVersion} — confirm compiler version for deployment");
        if (security.Count == 1) security.Insert(0, "Review all external/public functions for proper access modifiers");

        return JsonSerializer.Serialize(new
        {
            summary = $"{p.Name} is a {typeDesc} with {p.FunctionCount} functions, using {accessDesc}.",
            businessPurpose = p.IsErc721
                ? $"{p.Name} enables creation and transfer of unique digital assets on-chain, suitable for NFT marketplaces and digital collectibles."
                : p.IsErc20
                    ? $"{p.Name} manages fungible token balances, transfers, and approvals for DeFi, payments, or governance use cases."
                    : $"{p.Name} implements custom on-chain logic for decentralized application workflows.",
            beginnerExplanation = $"{p.Name} runs on the blockchain as autonomous code. It enforces rules automatically — who can mint tokens, how transfers work, and what operations require authorization — without a central intermediary.",
            securityConsiderations = security.ToArray()
        });
    }

    private static string AuditContract(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var findings = ContractAnalyzer.BuildAuditFindings(p);
        var critical = ChainMindEngineHelpers.CountSeverity(findings, "Critical");
        var high = ChainMindEngineHelpers.CountSeverity(findings, "High");
        var score = ContractAnalyzer.CalculateSecurityScore(p, findings.Length, critical, high);

        var assessment = score >= 85
            ? $"{p.Name} shows strong security posture. Address remaining findings before mainnet deployment."
            : score >= 70
                ? $"{p.Name} has moderate security concerns requiring attention before production use."
                : $"{p.Name} has significant security issues that must be resolved prior to deployment.";

        return JsonSerializer.Serialize(new { findings, securityScore = score, overallAssessment = assessment });
    }

    private static string GenerateDeployment(string systemPrompt, string contractCode)
    {
        var networkMatch = Regex.Match(systemPrompt, @"for\s+(\w+)", RegexOptions.IgnoreCase);
        var network = networkMatch.Success ? networkMatch.Groups[1].Value : "Ethereum";
        var p = ContractAnalyzer.Analyze(contractCode);

        var rpcMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ethereum"] = "https://eth-sepolia.g.alchemy.com/v2/YOUR_KEY",
            ["Polygon"] = "https://polygon-amoy.g.alchemy.com/v2/YOUR_KEY",
            ["Base"] = "https://base-sepolia.g.alchemy.com/v2/YOUR_KEY",
            ["Arbitrum"] = "https://arb-sepolia.g.alchemy.com/v2/YOUR_KEY"
        };
        var explorerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ethereum"] = "https://sepolia.etherscan.io",
            ["Polygon"] = "https://amoy.polygonscan.com",
            ["Base"] = "https://sepolia.basescan.org",
            ["Arbitrum"] = "https://sepolia.arbiscan.io"
        };

        rpcMap.TryGetValue(network, out var rpc);
        explorerMap.TryGetValue(network, out var explorer);
        rpc ??= "https://rpc.example.com";
        explorer ??= "https://explorer.example.com";

        return JsonSerializer.Serialize(new
        {
            network,
            steps = new[]
            {
                new { title = "Install Dependencies", description = "Initialize Hardhat project with OpenZeppelin contracts.", command = "npm init -y && npm install --save-dev hardhat @nomicfoundation/hardhat-toolbox @openzeppelin/contracts dotenv" },
                new { title = "Configure Environment", description = $"Set wallet private key and {network} RPC endpoint.", command = $"PRIVATE_KEY=0x...\n{network.ToUpper()}_RPC_URL={rpc}" },
                new { title = "Add Hardhat Config", description = $"Configure {network} testnet network in hardhat.config.js.", command = $"networks: {{ {network.ToLower()}: {{ url: process.env.{network.ToUpper()}_RPC_URL, accounts: [process.env.PRIVATE_KEY] }} }}" },
                new { title = "Compile Contracts", description = $"Compile {p.Name} with matching Solidity version.", command = "npx hardhat compile" },
                new { title = $"Deploy {p.Name} to {network}", description = $"Deploy contract to {network} testnet.", command = $"npx hardhat run scripts/deploy.js --network {network.ToLower()}" },
                new { title = "Verify on Explorer", description = "Verify source code on block explorer.", command = $"npx hardhat verify --network {network.ToLower()} <DEPLOYED_ADDRESS> \"<CONSTRUCTOR_ARGS>\"" }
            },
            verificationGuide = $"Visit {explorer}, search your contract address, click 'Verify & Publish', select Solidity {p.PragmaVersion}, and upload {p.Name}.sol with exact compiler settings."
        });
    }

    private static string GenerateTests(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var deployArgs = p.HasOwnable ? "owner.address" : "";

        return $$"""
const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("{{p.Name}}", function () {
  let contract, owner, user1, user2;

  beforeEach(async function () {
    [owner, user1, user2] = await ethers.getSigners();
    const Factory = await ethers.getContractFactory("{{p.Name}}");
    contract = await Factory.deploy({{deployArgs}});
    await contract.waitForDeployment();
  });

  describe("Deployment", function () {
    it("Should deploy successfully", async function () {
      expect(await contract.getAddress()).to.be.properAddress;
    });
  });

  describe("Unit Tests", function () {
    it("Should execute core functions correctly", async function () {
      // Add function-specific assertions based on contract ABI
      expect(contract).to.not.be.undefined;
    });
  });

  describe("Integration Tests", function () {
    it("Should interact with external contracts safely", async function () {
      // Integration test scenarios
    });
  });

  describe("Security Tests", function () {
    it("Should reject unauthorized access", async function () {
      // Access control tests
    });

    it("Should handle edge cases", async function () {
      // Zero address, overflow, reentrancy tests
    });
  });
});
""";
    }

    private static string AnalyzeGas(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var optimizations = new List<object>();
        var score = 80;

        if (p.HasRequireStrings && !p.HasCustomErrors)
        {
            optimizations.Add(new { title = "Replace Require Strings with Custom Errors", description = "String literals in require() increase bytecode size and revert gas.", estimatedSavings = "~200 gas per revert, ~2KB deployment" });
            score -= 5;
        }
        if (p.FunctionCount > 10)
        {
            optimizations.Add(new { title = "Split Contract Logic", description = $"{p.Name} has {p.FunctionCount} functions — consider modular libraries.", estimatedSavings = "~5-10% deployment gas" });
        }
        if (!p.HasEvents)
        {
            optimizations.Add(new { title = "Add Indexed Events", description = "Events enable efficient off-chain indexing without extra on-chain reads.", estimatedSavings = "Indirect — reduces future query costs" });
        }
        optimizations.Add(new { title = "Use immutable and constant", description = "Mark constructor-set values as immutable to avoid SLOAD costs.", estimatedSavings = "~2,100 gas per SLOAD avoided" });
        if (p.IsErc20)
            optimizations.Add(new { title = "Optimize ERC20 Storage", description = "Use OpenZeppelin ERC20 optimized implementation with packed storage.", estimatedSavings = "~15,000 gas on deployment" });

        var deployEstimate = p.IsErc721 ? "~2,500,000" : p.IsErc20 ? "~1,200,000" : "~800,000";
        var transferEstimate = p.IsErc20 ? "~51,000" : "~65,000";

        return JsonSerializer.Serialize(new
        {
            efficiencyScore = Math.Clamp(score, 40, 98),
            estimatedGasUsage = $"Deploy: ~{deployEstimate} gas | Transfer: ~{transferEstimate} gas | {p.FunctionCount} functions analyzed",
            optimizations,
            summary = $"Gas analysis for {p.Name} ({p.PragmaVersion}). {optimizations.Count} optimization opportunities identified."
        });
    }

    private static string GenerateAgentReport(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var findings = ContractAnalyzer.BuildAuditFindings(p);
        var critical = ChainMindEngineHelpers.CountSeverity(findings, "Critical");
        var high = ChainMindEngineHelpers.CountSeverity(findings, "High");
        var riskScore = Math.Clamp(100 - ContractAnalyzer.CalculateSecurityScore(p, findings.Length, critical, high), 0, 100);

        var recommendations = new List<object>();
        if (p.HasReentrancyRisk)
            recommendations.Add(new { priority = "Critical", category = "Reentrancy", recommendation = "Add ReentrancyGuard to all functions with external calls", reasoning = "External .call{value:} detected without nonReentrant modifier — classic reentrancy attack vector." });
        if (p.HasMint && p.HasOwnable)
            recommendations.Add(new { priority = "High", category = "Access Control", recommendation = "Implement multi-sig or timelock for owner actions", reasoning = "Single owner controls minting — compromised key risks unlimited token inflation." });
        recommendations.Add(new { priority = "Medium", category = "Testing", recommendation = "Achieve 100% branch coverage with Hardhat + Foundry fuzz tests", reasoning = "Comprehensive testing catches edge cases static analysis misses." });
        recommendations.Add(new { priority = "Low", category = "Documentation", recommendation = "Complete NatSpec for all public/external functions", reasoning = "Documentation reduces integration errors and accelerates audits." });

        var report = $"""
## Security Assessment: {p.Name}

### Contract Profile
- Type: {(p.IsErc721 ? "ERC721 NFT" : p.IsErc20 ? "ERC20 Token" : "Custom Contract")}
- Functions: {p.FunctionCount}
- Compiler: {p.PragmaVersion}
- Access Control: {(p.HasAccessControl ? "AccessControl" : p.HasOwnable ? "Ownable" : "None detected")}

### Findings Summary
- Critical: {critical} | High: {high} | Total: {findings.Length}

### Analysis
{(p.HasReentrancyRisk ? "⚠ Reentrancy risk identified\n" : "")}{(p.UsesTxOrigin ? "⚠ tx.origin usage detected\n" : "")}
{(critical == 0 && high == 0 ? "No critical vulnerabilities detected in static analysis." : "Issues require remediation before mainnet.")}

### Verdict
Risk Score: {riskScore}/100 — {(riskScore <= 30 ? "Low risk, proceed with testing" : riskScore <= 60 ? "Moderate risk, address findings" : "High risk, do not deploy")}
""";

        return JsonSerializer.Serialize(new
        {
            executiveSummary = $"Security review of {p.Name} complete. {findings.Length} findings identified with risk score {riskScore}/100. {(critical > 0 ? "Critical issues require immediate attention." : "No critical vulnerabilities in static analysis.")}",
            recommendations,
            detailedReport = report,
            riskScore
        });
    }

    private static string GenerateDocumentation(string code)
    {
        var p = ContractAnalyzer.Analyze(code);
        var functions = Regex.Matches(code, @"function\s+(\w+)\s*\([^)]*\)\s*(?:public|external|internal|private)?\s*(?:view|pure|payable)?\s*(?:returns\s*\([^)]*\))?\s*(?:override\s*)?\{?")
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        var funcDocs = string.Join("\n", functions.Select(f => $"### `{f}(...)`\nCore contract function — see NatSpec in source for parameters and return values.\n"));

        return $"""
# {p.Name} — Smart Contract Documentation

## Overview
{p.Name} is a {(p.IsErc721 ? "ERC721 NFT" : p.IsErc20 ? "ERC20 token" : "Solidity")} smart contract compiled with `{p.PragmaVersion}`.

## Contract Details
| Property | Value |
|----------|-------|
| Contract | {p.Name} |
| Functions | {p.FunctionCount} |
| Access Control | {(p.HasAccessControl ? "AccessControl" : p.HasOwnable ? "Ownable" : "Standard")} |
| Mint | {(p.HasMint ? "Yes" : "No")} |
| Burn | {(p.HasBurn ? "Yes" : "No")} |

## Functions
{funcDocs}

## Security Notes
- Solidity {p.PragmaVersion} overflow protection enabled
- {(p.HasOwnable ? "Owner privileges must be secured with hardware wallet or multi-sig" : "Review access control on privileged functions")}
{(p.HasReentrancyRisk ? "- ⚠ Reentrancy risk detected — review before deployment\n" : "")}

## Deployment
```bash
npx hardhat compile
npx hardhat run scripts/deploy.js --network sepolia
```

## Usage
```javascript
const contract = await ethers.getContractAt("{p.Name}", deployedAddress);
```
""";
    }
}

internal static partial class ChainMindEngineHelpers
{
    internal static int CountSeverity(object[] findings, string severity) =>
        findings.Count(f =>
        {
            var json = JsonSerializer.Serialize(f);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("severity", out var s) && s.GetString() == severity;
        });
}

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;

    public OpenAIProvider(HttpClient httpClient, IOptions<AISettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public string ProviderName => "OpenAI";

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settings.OpenAIModel,
            max_tokens = 4096,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}

public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;

    public ClaudeProvider(HttpClient httpClient, IOptions<AISettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public string ProviderName => "Claude";

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _settings.ClaudeModel,
            max_tokens = 4096,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("messages", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;
    }
}
