using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Categorias;

namespace ServicoPalavra.Api.Controllers;

public sealed class CategoriasController : ApiControllerBase
{
    private readonly ICategoriaService _categorias;

    public CategoriasController(ICategoriaService categorias) => _categorias = categorias;

    [HttpGet("api/categorias")]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        OkResponse(await _categorias.ListAsync(cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/categorias")]
    public async Task<IActionResult> Create(CategoriaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.CreateAsync(request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/categorias/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoriaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.UpdateAsync(id, request, cancellationToken));
}
