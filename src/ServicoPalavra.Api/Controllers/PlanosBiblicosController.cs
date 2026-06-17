using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.PlanosBiblicos;

namespace ServicoPalavra.Api.Controllers;

[Authorize]
public sealed class PlanosBiblicosController : ApiControllerBase
{
    private readonly IPlanoBiblicoService _planos;
    private readonly ICurrentUser _currentUser;

    public PlanosBiblicosController(IPlanoBiblicoService planos, ICurrentUser currentUser)
    {
        _planos = planos;
        _currentUser = currentUser;
    }

    [HttpGet("api/planos-biblicos/ativo")]
    public async Task<IActionResult> Ativo(CancellationToken cancellationToken) =>
        OkResponse(await _planos.GetAtivoAsync(CurrentUserId(_currentUser), cancellationToken));

    [HttpGet("api/planos-biblicos/me/ativo")]
    public async Task<IActionResult> MeuAtivo(CancellationToken cancellationToken) =>
        OkResponse(await _planos.GetAtivoAsync(CurrentUserId(_currentUser), cancellationToken));

    [HttpGet("api/planos-biblicos/me/historico")]
    public async Task<IActionResult> Historico(CancellationToken cancellationToken) =>
        OkResponse(await _planos.GetHistoricoAsync(CurrentUserId(_currentUser), cancellationToken));

    [HttpPost("api/planos-biblicos")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> Criar(CriarPlanoBiblicoRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _planos.CriarAsync(CurrentUserId(_currentUser), request, cancellationToken));

    [HttpPost("api/planos-biblicos/alterar")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> Alterar(AlterarPlanoBiblicoRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _planos.AlterarAsync(CurrentUserId(_currentUser), request, cancellationToken));

    [HttpGet("api/planos-biblicos/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _planos.GetAsync(CurrentUserId(_currentUser), id, cancellationToken));

    [HttpGet("api/planos-biblicos/{id:guid}/dias")]
    public async Task<IActionResult> Dias(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _planos.ListDiasAsync(CurrentUserId(_currentUser), id, cancellationToken));

    [HttpGet("api/planos-biblicos/progresso/posicao-atual")]
    public async Task<IActionResult> Posicao(CancellationToken cancellationToken) =>
        OkResponse(await _planos.GetPosicaoAtualAsync(CurrentUserId(_currentUser), cancellationToken));

    [HttpPost("api/planos-biblicos/dias/{diaId:guid}/concluir")]
    public async Task<IActionResult> ConcluirDia(Guid diaId, CancellationToken cancellationToken) =>
        OkResponse(await _planos.ConcluirDiaAsync(CurrentUserId(_currentUser), diaId, cancellationToken), "Dia concluido.");

    [HttpPost("api/planos-biblicos/dias/{diaId:guid}/desmarcar")]
    public async Task<IActionResult> DesmarcarDia(Guid diaId, CancellationToken cancellationToken) =>
        OkResponse(await _planos.DesmarcarDiaAsync(CurrentUserId(_currentUser), diaId, cancellationToken), "Dia desmarcado.");
}
