using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Dashboard;

namespace ServicoPalavra.Api.Controllers;

[Authorize]
public sealed class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboard;
    private readonly ICurrentUser _currentUser;

    public DashboardController(IDashboardService dashboard, ICurrentUser currentUser)
    {
        _dashboard = dashboard;
        _currentUser = currentUser;
    }

    [HttpGet("api/dashboard/me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) =>
        OkResponse(await _dashboard.GetMeAsync(CurrentUserId(_currentUser), cancellationToken));
}
