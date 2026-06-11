using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Conteudos;

namespace ServicoPalavra.Api.Controllers;

public sealed class ConteudosController : ApiControllerBase
{
    private readonly IConteudoService _conteudos;
    private readonly ICurrentUser _currentUser;

    public ConteudosController(IConteudoService conteudos, ICurrentUser currentUser)
    {
        _conteudos = conteudos;
        _currentUser = currentUser;
    }

    [HttpGet("api/conteudos")]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.ListAsync(cancellationToken));

    [HttpGet("api/conteudos/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.GetAsync(id, false, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/conteudos")]
    public async Task<IActionResult> Create(ConteudoRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.CreateAsync(request, CurrentUserId(_currentUser), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/conteudos/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ConteudoRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPatch("api/admin/conteudos/{id:guid}/publicar")]
    public async Task<IActionResult> Publicar(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.PublicarAsync(id, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPatch("api/admin/conteudos/{id:guid}/despublicar")]
    public async Task<IActionResult> Despublicar(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _conteudos.DespublicarAsync(id, cancellationToken));
}
