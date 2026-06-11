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
    public async Task<IActionResult> Concluir(Guid id, CancellationToken cancellationToken)
    {
        await _progressos.ConcluirConteudoAsync(CurrentUserId(_currentUser), id, cancellationToken);
        return OkResponse("Conteudo concluido.");
    }
}
