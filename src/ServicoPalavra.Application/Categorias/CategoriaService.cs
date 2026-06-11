using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Categorias;

public sealed class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categorias;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaService(ICategoriaRepository categorias, IUnitOfWork unitOfWork)
    {
        _categorias = categorias;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListAsync(CancellationToken cancellationToken = default) =>
        (await _categorias.ListAtivasAsync(cancellationToken)).Select(ToResponse).ToList();

    public async Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken cancellationToken = default)
    {
        var slug = BuildSlug(request);
        if (await _categorias.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new AppException("Slug de categoria ja existe.");
        }

        var categoria = new CategoriaConteudo
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Slug = slug,
            Descricao = request.Descricao,
            Cor = request.Cor,
            Icone = request.Icone,
            Ordem = request.Ordem,
            Ativo = request.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        _categorias.Add(categoria);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(categoria);
    }

    public async Task<CategoriaResponse> UpdateAsync(Guid id, CategoriaRequest request, CancellationToken cancellationToken = default)
    {
        var categoria = await _categorias.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Categoria nao encontrada.", 404);

        var slug = BuildSlug(request);
        if (await _categorias.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new AppException("Slug de categoria ja existe.");
        }

        categoria.Nome = request.Nome.Trim();
        categoria.Slug = slug;
        categoria.Descricao = request.Descricao;
        categoria.Cor = request.Cor;
        categoria.Icone = request.Icone;
        categoria.Ordem = request.Ordem;
        categoria.Ativo = request.Ativo;
        categoria.AtualizadoEm = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(categoria);
    }

    private static string BuildSlug(CategoriaRequest request) => string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Nome) : Slug.From(request.Slug);
    private static CategoriaResponse ToResponse(CategoriaConteudo categoria) =>
        new(categoria.Id, categoria.Nome, categoria.Slug, categoria.Descricao, categoria.Cor, categoria.Icone, categoria.Ordem);
}
