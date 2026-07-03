using ChainMind.Core.Interfaces;
using ChainMind.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChainMind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IAnalyticsRepository _analytics;

    public DashboardController(IAnalyticsRepository analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats(CancellationToken ct)
    {
        var stats = await _analytics.GetStatsAsync(ct);
        return Ok(stats);
    }

    [HttpGet("projects")]
    public async Task<ActionResult<IReadOnlyList<ProjectItem>>> GetProjects(CancellationToken ct)
    {
        var projects = await _analytics.GetRecentProjectsAsync(ct);
        return Ok(projects);
    }

    [HttpGet("templates")]
    public async Task<ActionResult<IReadOnlyList<TemplateItem>>> GetTemplates(CancellationToken ct)
    {
        var templates = await _analytics.GetTemplatesAsync(ct);
        return Ok(templates);
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionItem>>> GetSessions(CancellationToken ct)
    {
        var sessions = await _analytics.GetSessionsAsync(ct);
        return Ok(sessions);
    }
}
