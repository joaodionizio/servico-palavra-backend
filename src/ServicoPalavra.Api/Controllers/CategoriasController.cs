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
    [HttpGet("api/admin/categorias")]
    public async Task<IActionResult> ListAdmin(CancellationToken cancellationToken) =>
        OkResponse(await _categorias.ListAdminAsync(cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/categorias/{id:guid}")]
    public async Task<IActionResult> GetAdmin(Guid id, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.GetAdminAsync(id, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/categorias")]
    public async Task<IActionResult> Create(CategoriaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.CreateAsync(request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/categorias/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoriaRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPatch("api/admin/categorias/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, CategoriaStatusRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _categorias.UpdateStatusAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/categorias/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _categorias.DeleteAsync(id, cancellationToken);
        return OkResponse("Categoria excluida com sucesso.");
    }
}
