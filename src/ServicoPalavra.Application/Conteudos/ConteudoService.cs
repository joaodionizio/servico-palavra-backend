using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Conteudos;

public sealed class ConteudoService : IConteudoService
{
    private readonly IConteudoRepository _conteudos;
    private readonly ICategoriaRepository _categorias;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ConteudoService(IConteudoRepository conteudos, ICategoriaRepository categorias, IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _conteudos = conteudos;
        _categorias = categorias;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ConteudoResponse>> ListAsync(CancellationToken cancellationToken = default) =>
        (await _conteudos.ListPublicadosAsync(cancellationToken)).Select(ToResponse).ToList();

    public async Task<ConteudoResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) =>
        ToResponse(await _conteudos.GetByIdAsync(id, includePrivate, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404));

    public async Task<ConteudoResponse> CreateAsync(ConteudoRequest request, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        ExternalUrlValidator.ValidateConteudoUrl(request.Url, request.Origem);
        ExternalUrlValidator.ValidateOptionalThumbnail(request.UrlThumbnail);

        var slug = BuildSlug(request);
        if (await _conteudos.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new AppException("Slug de conteudo ja existe.");
        }

        var categoria = await _categorias.GetByIdAsync(request.CategoriaConteudoId, cancellationToken)
            ?? throw new AppException("Categoria nao encontrada.", 404);
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuario criador nao encontrado.", 404);

        var conteudo = new Conteudo
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            Slug = slug,
            Descricao = request.Descricao,
            Resumo = request.Resumo,
            Tipo = request.Tipo,
            Origem = request.Origem,
            Url = request.Url.Trim(),
            UrlThumbnail = request.UrlThumbnail,
            DuracaoMinutos = request.DuracaoMinutos,
            CategoriaConteudoId = categoria.Id,
            CategoriaConteudo = categoria,
            CriadoPorUsuarioId = usuario.Id,
            CriadoPorUsuario = usuario,
            Publicado = request.Publicado,
            Destaque = request.Destaque,
            Ordem = request.Ordem,
            PublicadoEm = request.Publicado ? DateTime.UtcNow : null,
            CriadoEm = DateTime.UtcNow
        };

        _conteudos.Add(conteudo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    public async Task<ConteudoResponse> UpdateAsync(Guid id, ConteudoRequest request, CancellationToken cancellationToken = default)
    {
        ExternalUrlValidator.ValidateConteudoUrl(request.Url, request.Origem);
        ExternalUrlValidator.ValidateOptionalThumbnail(request.UrlThumbnail);

        var conteudo = await _conteudos.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);
        var slug = BuildSlug(request);

        if (await _conteudos.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new AppException("Slug de conteudo ja existe.");
        }

        _ = await _categorias.GetByIdAsync(request.CategoriaConteudoId, cancellationToken)
            ?? throw new AppException("Categoria nao encontrada.", 404);

        conteudo.Titulo = request.Titulo.Trim();
        conteudo.Slug = slug;
        conteudo.Descricao = request.Descricao;
        conteudo.Resumo = request.Resumo;
        conteudo.Tipo = request.Tipo;
        conteudo.Origem = request.Origem;
        conteudo.Url = request.Url.Trim();
        conteudo.UrlThumbnail = request.UrlThumbnail;
        conteudo.DuracaoMinutos = request.DuracaoMinutos;
        conteudo.CategoriaConteudoId = request.CategoriaConteudoId;
        conteudo.PublicadoEm = !conteudo.Publicado && request.Publicado ? DateTime.UtcNow : conteudo.PublicadoEm;
        conteudo.Publicado = request.Publicado;
        conteudo.Destaque = request.Destaque;
        conteudo.Ordem = request.Ordem;
        conteudo.AtualizadoEm = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    public async Task<ConteudoResponse> PublicarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conteudo = await _conteudos.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);
        conteudo.Publicado = true;
        conteudo.PublicadoEm ??= DateTime.UtcNow;
        conteudo.AtualizadoEm = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    public async Task<ConteudoResponse> DespublicarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conteudo = await _conteudos.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);
        conteudo.Publicado = false;
        conteudo.AtualizadoEm = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    private static string BuildSlug(ConteudoRequest request) => string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Titulo) : Slug.From(request.Slug);
    private static ConteudoResponse ToResponse(Conteudo conteudo) =>
        new(conteudo.Id, conteudo.Titulo, conteudo.Slug, conteudo.Descricao, conteudo.Resumo, conteudo.Tipo, conteudo.Origem, conteudo.Url, conteudo.UrlThumbnail, conteudo.DuracaoMinutos, conteudo.CategoriaConteudoId, conteudo.CategoriaConteudo.Nome, conteudo.Publicado, conteudo.Destaque, conteudo.Ordem, conteudo.PublicadoEm);
}
