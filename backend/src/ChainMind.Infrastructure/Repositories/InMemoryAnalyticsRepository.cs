using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;

namespace ChainMind.Infrastructure.Repositories;

public class InMemoryAnalyticsRepository : IAnalyticsRepository
{
    private int _contractsGenerated;
    private int _auditsCompleted;
    private readonly List<int> _auditScores = new();
    private readonly List<ProjectItem> _projects = new();
    private readonly List<SessionItem> _sessions = new();

    private readonly List<TemplateItem> _templates = new()
    {
        new("t1", "ERC20 Token", "Standard fungible token with mint/burn", "Token"),
        new("t2", "ERC721 NFT", "Non-fungible token collection", "NFT"),
        new("t3", "Crop Insurance", "Region-based weather insurance for farmers", "DeFi"),
        new("t4", "Staking Contract", "Token staking with rewards", "DeFi"),
        new("t5", "DAO Governance", "On-chain voting and proposals", "Governance"),
        new("t6", "HealingToken", "Non-profit health initiative ERC-20", "Token"),
        new("t7", "Escrow Contract", "Secure payment escrow", "Utility"),
    };

    private readonly object _lock = new();

    public Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var avg = _auditScores.Count > 0 ? _auditScores.Average() : 0;
            return Task.FromResult(new DashboardStats(
                _contractsGenerated,
                _auditsCompleted,
                Math.Round(avg, 1),
                _sessions.Count
            ));
        }
    }

    public Task IncrementContractsGeneratedAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock) { _contractsGenerated++; }
        return Task.CompletedTask;
    }

    public Task IncrementAuditsCompletedAsync(int securityScore, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _auditsCompleted++;
            _auditScores.Add(securityScore);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProjectItem>> GetRecentProjectsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<ProjectItem>>(_projects.OrderByDescending(p => p.CreatedAt).Take(10).ToList());
        }
    }

    public Task<IReadOnlyList<TemplateItem>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TemplateItem>>(_templates.AsReadOnly());
    }

    public Task<IReadOnlyList<SessionItem>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<SessionItem>>(_sessions.OrderByDescending(s => s.CreatedAt).Take(10).ToList());
        }
    }

    public Task AddProjectAsync(ProjectItem project, CancellationToken cancellationToken = default)
    {
        lock (_lock) { _projects.Insert(0, project); }
        return Task.CompletedTask;
    }

    public Task AddSessionAsync(SessionItem session, CancellationToken cancellationToken = default)
    {
        lock (_lock) { _sessions.Insert(0, session); }
        return Task.CompletedTask;
    }
}
