using ChainMind.Core.Models;

namespace ChainMind.Core.Interfaces;

public interface IAnalyticsRepository
{
    Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);
    Task IncrementContractsGeneratedAsync(CancellationToken cancellationToken = default);
    Task IncrementAuditsCompletedAsync(int securityScore, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectItem>> GetRecentProjectsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TemplateItem>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionItem>> GetSessionsAsync(CancellationToken cancellationToken = default);
    Task AddProjectAsync(ProjectItem project, CancellationToken cancellationToken = default);
    Task AddSessionAsync(SessionItem session, CancellationToken cancellationToken = default);
}
