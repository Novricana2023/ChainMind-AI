using System.Text.RegularExpressions;

namespace ChainMind.Application.Validation;

public static class ContractValidator
{
    public static IReadOnlyList<string> Validate(string code)
    {
        var issues = new List<string>();

        if (!code.Contains("SPDX-License-Identifier", StringComparison.Ordinal))
            issues.Add("Missing SPDX-License-Identifier comment");

        if (!Regex.IsMatch(code, @"pragma\s+solidity\s+\^?0\.8"))
            issues.Add("Missing or invalid pragma solidity ^0.8.x");

        if (Regex.IsMatch(code, @"struct\s+\w+\s*\{[^}]*mapping\s*\(", RegexOptions.Singleline))
            issues.Add("Mapping inside struct — use contract-level nested mappings instead");

        if (code.Contains("security/ReentrancyGuard", StringComparison.Ordinal))
            issues.Add("Wrong ReentrancyGuard import — use @openzeppelin/contracts/utils/ReentrancyGuard.sol (OZ v5)");

        if (code.Contains("Counters.sol", StringComparison.Ordinal))
            issues.Add("Counters.sol is removed in OpenZeppelin v5 — use uint256 counter");

        if (Regex.IsMatch(code, @"\.call\{value:") && !code.Contains("nonReentrant", StringComparison.Ordinal))
            issues.Add("External ETH call without nonReentrant — add ReentrancyGuard");

        if (Regex.IsMatch(code, @"\.call\{value:") && !Regex.IsMatch(code, @"balance\s*<|InsufficientFunds|insufficient", RegexOptions.IgnoreCase))
            issues.Add("ETH payout without balance check — verify address(this).balance before transfer");

        return issues;
    }
}
