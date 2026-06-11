using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Trilhas;

namespace ServicoPalavra.Api.Controllers;

public sealed class TrilhasController : ApiControllerBase
{
    private readonly ITrilhaService _trilhas;
    private readonly ICurrentUser _currentUser;

    public TrilhasController(ITrilhaService trilhas, ICurrentUser currentUser)
    {
        _trilhas = trilhas;
        _currentUser = currentUser;
    }

    [HttpGet("api/trilhas")]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        OkResponse(await _trilhas.ListAsync(cancellationToken));

    [HttpGet("api/trilhas/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _trilhas.GetAsync(id, false, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/trilhas")]
    public async Task<IActionResult> Create(TrilhaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _trilhas.CreateAsync(request, CurrentUserId(_currentUser), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/trilhas/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TrilhaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _trilhas.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/trilhas/{id:guid}/conteudos")]
    public async Task<IActionResult> AddConteudo(Guid id, AddTrilhaConteudoRequest request, CancellationToken cancellationToken)
    {
        await _trilhas.AddConteudoAsync(id, request, cancellationToken);
        return OkResponse("Conteudo adicionado a trilha.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/trilhas/{id:guid}/conteudos/ordem")]
    public async Task<IActionResult> Reordenar(Guid id, ReordenarTrilhaConteudoRequest request, CancellationToken cancellationToken)
    {
        await _trilhas.ReordenarConteudosAsync(id, request, cancellationToken);
        return OkResponse("Ordem atualizada.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/trilhas/{id:guid}/conteudos/{conteudoId:guid}")]
    public async Task<IActionResult> RemoveConteudo(Guid id, Guid conteudoId, CancellationToken cancellationToken)
    {
        await _trilhas.RemoveConteudoAsync(id, conteudoId, cancellationToken);
        return OkResponse("Conteudo removido da trilha.");
    }
}
