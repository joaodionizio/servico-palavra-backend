using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Favoritos;

namespace ServicoPalavra.Api.Controllers;

[Authorize]
public sealed class FavoritosController : ApiControllerBase
{
    private readonly IFavoritoService _favoritos;
    private readonly ICurrentUser _currentUser;

    public FavoritosController(IFavoritoService favoritos, ICurrentUser currentUser)
    {
        _favoritos = favoritos;
        _currentUser = currentUser;
    }

    [HttpGet("api/favoritos")]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        OkResponse(await _favoritos.ListAsync(CurrentUserId(_currentUser), cancellationToken));

    [HttpPost("api/conteudos/{id:guid}/favoritar")]
    public async Task<IActionResult> FavoritarConteudo(Guid id, CancellationToken cancellationToken) =>
        await Favoritar(id, cancellationToken);

    [HttpPost("api/favoritos/{conteudoId:guid}")]
    public async Task<IActionResult> Favoritar(Guid conteudoId, CancellationToken cancellationToken)
    {
        await _favoritos.FavoritarAsync(CurrentUserId(_currentUser), conteudoId, cancellationToken);
        return OkResponse("Conteudo favoritado.");
    }

    [HttpDelete("api/conteudos/{id:guid}/favoritar")]
    public async Task<IActionResult> RemoverFavoritoConteudo(Guid id, CancellationToken cancellationToken) =>
        await Remover(id, cancellationToken);

    [HttpDelete("api/favoritos/{conteudoId:guid}")]
    public async Task<IActionResult> Remover(Guid conteudoId, CancellationToken cancellationToken)
    {
        await _favoritos.RemoverAsync(CurrentUserId(_currentUser), conteudoId, cancellationToken);
        return OkResponse("Favorito removido.");
    }
}
