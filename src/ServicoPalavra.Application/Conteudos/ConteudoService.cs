using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Conteudos;

public sealed class ConteudoService : IConteudoService
{
    private readonly IConteudoRepository _conteudos;
    private readonly ICategoriaRepository _categorias;
    private readonly IFavoritoRepository _favoritos;
    private readonly IProgressoRepository _progressos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ConteudoService(IConteudoRepository conteudos, ICategoriaRepository categorias, IFavoritoRepository favoritos, IProgressoRepository progressos, IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _conteudos = conteudos;
        _categorias = categorias;
        _favoritos = favoritos;
        _progressos = progressos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<ConteudoResumoResponse>> ListAsync(ConteudoListQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Pagina);
        var pageSize = Math.Clamp(query.TamanhoPagina, 1, 50);
        var skip = (page - 1) * pageSize;
        var (items, total) = await _conteudos.ListPublicadosAsync(query.Busca, query.CategoriaSlug, query.Tipo, skip, pageSize, cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResponse<ConteudoResumoResponse>(items.Select(ToResumoResponse).ToList(), page, pageSize, total, totalPages);
    }

    public async Task<PagedResponse<ConteudoAdminResumoResponse>> ListAdminAsync(ConteudoAdminListQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Pagina);
        var pageSize = Math.Clamp(query.TamanhoPagina, 1, 100);
        var skip = (page - 1) * pageSize;
        var (items, total) = await _conteudos.ListAdminAsync(query.Busca, query.CategoriaSlug, query.Tipo, query.Publicado, skip, pageSize, cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResponse<ConteudoAdminResumoResponse>(items.Select(ToAdminResumoResponse).ToList(), page, pageSize, total, totalPages);
    }

    public async Task<ConteudoResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) =>
        ToResponse(await _conteudos.GetByIdAsync(id, includePrivate, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404));

    public async Task<ConteudoAdminDetalheResponse> GetAdminAsync(Guid id, CancellationToken cancellationToken = default) =>
        ToAdminDetalheResponse(await _conteudos.GetByIdWithMaterialsAsync(id, true, false, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404));

    public async Task<ConteudoDetalheResponse> GetBySlugAsync(string slug, Guid? usuarioId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = Slug.From(slug);
        var conteudo = await _conteudos.GetBySlugAsync(normalizedSlug, false, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);

        var favorito = false;
        var concluido = false;

        if (usuarioId.HasValue)
        {
            favorito = await _favoritos.GetAsync(usuarioId.Value, conteudo.Id, cancellationToken) is not null;
            concluido = (await _progressos.GetConteudoAsync(usuarioId.Value, conteudo.Id, cancellationToken))?.Status == Domain.Enums.StatusProgressoConteudo.Concluido;
        }

        return ToDetalheResponse(conteudo, favorito, concluido);
    }

    public async Task<ConteudoResponse> CreateAsync(ConteudoRequest request, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        ValidateRequiredFields(request);
        ExternalUrlValidator.ValidateConteudoUrl(request.Url, request.Origem);
        ExternalUrlValidator.ValidateOptionalThumbnail(request.UrlThumbnail);
        ValidateMateriaisApoio(request.MateriaisApoio);

        var slug = BuildSlug(request);
        if (await _conteudos.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new AppException("Slug de conteudo ja existe.");
        }

        var categoria = await GetCategoriaAsync(request.CategoriaConteudoId, cancellationToken);
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuario criador nao encontrado.", 404);

        var now = DateTime.UtcNow;
        var publicado = request.Publicado ?? true;
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
            CategoriaConteudoId = categoria?.Id,
            CategoriaConteudo = categoria,
            CriadoPorUsuarioId = usuario.Id,
            CriadoPorUsuario = usuario,
            Publicado = publicado,
            Destaque = request.Destaque ?? false,
            Ordem = request.Ordem,
            PublicadoEm = publicado ? now : null,
            CriadoEm = now,
            MateriaisApoio = BuildMateriaisApoio(request.MateriaisApoio, now)
        };

        _conteudos.Add(conteudo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    public async Task<ConteudoResponse> UpdateAsync(Guid id, ConteudoRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequiredFields(request);
        ExternalUrlValidator.ValidateConteudoUrl(request.Url, request.Origem);
        ExternalUrlValidator.ValidateOptionalThumbnail(request.UrlThumbnail);
        ValidateMateriaisApoio(request.MateriaisApoio);

        var conteudo = await _conteudos.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);
        var slug = BuildSlug(request);

        if (await _conteudos.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new AppException("Slug de conteudo ja existe.");
        }

        var categoria = await GetCategoriaAsync(request.CategoriaConteudoId, cancellationToken);

        var now = DateTime.UtcNow;
        var publicado = request.Publicado ?? conteudo.Publicado;
        conteudo.Titulo = request.Titulo.Trim();
        conteudo.Slug = slug;
        conteudo.Descricao = request.Descricao;
        conteudo.Resumo = request.Resumo;
        conteudo.Tipo = request.Tipo;
        conteudo.Origem = request.Origem;
        conteudo.Url = request.Url.Trim();
        conteudo.UrlThumbnail = request.UrlThumbnail;
        conteudo.DuracaoMinutos = request.DuracaoMinutos;
        conteudo.CategoriaConteudoId = categoria?.Id;
        conteudo.CategoriaConteudo = categoria;
        conteudo.PublicadoEm = !conteudo.Publicado && publicado ? now : conteudo.PublicadoEm;
        conteudo.Publicado = publicado;
        conteudo.Destaque = request.Destaque ?? conteudo.Destaque;
        conteudo.Ordem = request.Ordem;
        conteudo.AtualizadoEm = now;

        if (request.MateriaisApoio is not null)
        {
            await _conteudos.DeleteMateriaisApoioAsync(conteudo.Id, cancellationToken);

            foreach (var material in BuildMateriaisApoio(request.MateriaisApoio, now))
            {
                material.ConteudoId = conteudo.Id;
                _conteudos.AddMaterialApoio(material);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(conteudo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conteudo = await _conteudos.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado.", 404);

        _conteudos.Remove(conteudo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConteudoResponse> UpdatePublicacaoAsync(Guid id, ConteudoPublicacaoRequest request, CancellationToken cancellationToken = default) =>
        request.Publicado ? await PublicarAsync(id, cancellationToken) : await DespublicarAsync(id, cancellationToken);

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
    private static void ValidateRequiredFields(ConteudoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
        {
            throw new AppException("Titulo e obrigatorio.");
        }

        if (!Enum.IsDefined(typeof(TipoConteudo), request.Tipo))
        {
            throw new AppException("Tipo de conteudo invalido.");
        }

        if (!Enum.IsDefined(typeof(OrigemConteudo), request.Origem))
        {
            throw new AppException("Origem de conteudo invalida.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new AppException("URL do conteudo e obrigatoria.");
        }
    }

    private async Task<CategoriaConteudo?> GetCategoriaAsync(Guid? categoriaConteudoId, CancellationToken cancellationToken)
    {
        if (!categoriaConteudoId.HasValue || categoriaConteudoId.Value == Guid.Empty)
        {
            return null;
        }

        return await _categorias.GetByIdAsync(categoriaConteudoId.Value, cancellationToken)
            ?? throw new AppException("Categoria nao encontrada.", 404);
    }

    private static ConteudoResumoResponse ToResumoResponse(Conteudo conteudo) =>
        new(conteudo.Id, conteudo.Titulo, conteudo.Slug, conteudo.Resumo, conteudo.Tipo, conteudo.Origem, conteudo.UrlThumbnail, conteudo.DuracaoMinutos, conteudo.CategoriaConteudoId, conteudo.CategoriaConteudo?.Nome, conteudo.Destaque, conteudo.PublicadoEm);
    private static ConteudoAdminResumoResponse ToAdminResumoResponse(Conteudo conteudo) =>
        new(conteudo.Id, conteudo.Titulo, conteudo.Slug, conteudo.Resumo, conteudo.Tipo, conteudo.Origem, conteudo.UrlThumbnail, conteudo.DuracaoMinutos, conteudo.CategoriaConteudoId, conteudo.CategoriaConteudo?.Nome, conteudo.Publicado, conteudo.Destaque, conteudo.Ordem, conteudo.PublicadoEm);
    private static ConteudoResponse ToResponse(Conteudo conteudo) =>
        new(conteudo.Id, conteudo.Titulo, conteudo.Slug, conteudo.Descricao, conteudo.Resumo, conteudo.Tipo, conteudo.Origem, conteudo.Url, conteudo.UrlThumbnail, conteudo.DuracaoMinutos, conteudo.CategoriaConteudoId, conteudo.CategoriaConteudo?.Nome, conteudo.Publicado, conteudo.Destaque, conteudo.Ordem, conteudo.PublicadoEm);
    private static ConteudoDetalheResponse ToDetalheResponse(Conteudo conteudo, bool favorito, bool concluido) =>
        new(
            conteudo.Id,
            conteudo.Titulo,
            conteudo.Slug,
            conteudo.Descricao,
            conteudo.Resumo,
            conteudo.Tipo,
            conteudo.Origem,
            conteudo.Url,
            conteudo.UrlThumbnail,
            conteudo.DuracaoMinutos,
            conteudo.CategoriaConteudoId,
            conteudo.CategoriaConteudo?.Nome,
            conteudo.Publicado,
            conteudo.Destaque,
            conteudo.Ordem,
            conteudo.PublicadoEm,
            conteudo.MateriaisApoio
                .Where(x => x.Ativo)
                .OrderBy(x => x.Ordem)
                .ThenBy(x => x.Titulo)
                .Select(x => new MaterialApoioResponse(x.Id, x.Titulo, x.Descricao, x.Tipo, x.Url, x.Ordem))
                .ToList(),
            favorito,
            concluido);
    private static ConteudoAdminDetalheResponse ToAdminDetalheResponse(Conteudo conteudo) =>
        new(
            conteudo.Id,
            conteudo.Titulo,
            conteudo.Slug,
            conteudo.Descricao,
            conteudo.Resumo,
            conteudo.Tipo,
            conteudo.Origem,
            conteudo.Url,
            conteudo.UrlThumbnail,
            conteudo.DuracaoMinutos,
            conteudo.CategoriaConteudoId,
            conteudo.CategoriaConteudo?.Nome,
            conteudo.Publicado,
            conteudo.Destaque,
            conteudo.Ordem,
            conteudo.PublicadoEm,
            conteudo.MateriaisApoio
                .OrderBy(x => x.Ordem)
                .ThenBy(x => x.Titulo)
                .Select(x => new MaterialApoioAdminResponse(x.Id, x.Titulo, x.Descricao, x.Tipo, x.Url, x.Ordem, x.Ativo))
                .ToList());

    private static void ValidateMateriaisApoio(IReadOnlyList<MaterialApoioRequest>? materiais)
    {
        if (materiais is null)
        {
            return;
        }

        foreach (var material in materiais)
        {
            ExternalUrlValidator.ValidateMaterialUrl(material.Url);
        }
    }

    private static List<MaterialApoio> BuildMateriaisApoio(IReadOnlyList<MaterialApoioRequest>? materiais, DateTime now) =>
        materiais?
            .Select((material, index) => new MaterialApoio
            {
                Id = Guid.NewGuid(),
                Titulo = material.Titulo.Trim(),
                Descricao = material.Descricao,
                Tipo = material.Tipo,
                Url = material.Url.Trim(),
                Ordem = material.Ordem ?? index + 1,
                Ativo = material.Ativo ?? true,
                CriadoEm = now
            })
            .ToList() ?? [];
}
