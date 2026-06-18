using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Progresso;

namespace ServicoPalavra.Api.Controllers;

[Authorize]
public sealed class ProgressoController : ApiControllerBase
{
    private readonly IProgressoService _progressos;
    private readonly ICurrentUser _currentUser;

    public ProgressoController(IProgressoService progressos, ICurrentUser currentUser)
    {
        _progressos = progressos;
        _currentUser = currentUser;
    }

    [HttpPost("api/conteudos/{id:guid}/concluir")]
    public async Task<IActionResult> ConcluirConteudo(Guid id, CancellationToken cancellationToken) =>
        await Concluir(id, cancellationToken);

    [HttpPost("api/progresso/conteudos/{conteudoId:guid}/concluir")]
    public async Task<IActionResult> Concluir(Guid conteudoId, CancellationToken cancellationToken)
    {
        await _progressos.ConcluirConteudoAsync(CurrentUserId(_currentUser), conteudoId, cancellationToken);
        return OkResponse("Conteudo concluido.");
    }

    [HttpDelete("api/progresso/conteudos/{conteudoId:guid}/concluir")]
    public async Task<IActionResult> Desmarcar(Guid conteudoId, CancellationToken cancellationToken)
    {
        await _progressos.DesmarcarConclusaoConteudoAsync(CurrentUserId(_currentUser), conteudoId, cancellationToken);
        return OkResponse("Conclusao removida.");
    }
}
