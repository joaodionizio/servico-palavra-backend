using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Trilhas;

public sealed class TrilhaService : ITrilhaService
{
    private readonly ITrilhaRepository _trilhas;
    private readonly IConteudoRepository _conteudos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public TrilhaService(ITrilhaRepository trilhas, IConteudoRepository conteudos, IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _trilhas = trilhas;
        _conteudos = conteudos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TrilhaResponse>> ListAsync(CancellationToken cancellationToken = default) =>
        (await _trilhas.ListPublicadasAsync(cancellationToken)).Select(ToResponse).ToList();

    public async Task<TrilhaResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) =>
        ToResponse(await _trilhas.GetByIdAsync(id, includePrivate, cancellationToken) ?? throw new AppException("Trilha nao encontrada.", 404));

    public async Task<TrilhaResponse> CreateAsync(TrilhaRequest request, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var slug = BuildSlug(request);
        if (await _trilhas.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new AppException("Slug de trilha ja existe.");
        }

        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuario criador nao encontrado.", 404);
        var trilha = new TrilhaFormacao
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            Slug = slug,
            Descricao = request.Descricao,
            Resumo = request.Resumo,
            ImagemUrl = request.ImagemUrl,
            Publicado = request.Publicado,
            Destaque = request.Destaque,
            Ordem = request.Ordem,
            CriadoPorUsuarioId = usuario.Id,
            CriadoPorUsuario = usuario,
            PublicadoEm = request.Publicado ? DateTime.UtcNow : null,
            CriadoEm = DateTime.UtcNow
        };

        _trilhas.Add(trilha);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(trilha);
    }

    public async Task<TrilhaResponse> UpdateAsync(Guid id, TrilhaRequest request, CancellationToken cancellationToken = default)
    {
        var trilha = await _trilhas.GetByIdAsync(id, true, cancellationToken)
            ?? throw new AppException("Trilha nao encontrada.", 404);
        var slug = BuildSlug(request);
        if (await _trilhas.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new AppException("Slug de trilha ja existe.");
        }

        trilha.Titulo = request.Titulo.Trim();
        trilha.Slug = slug;
        trilha.Descricao = request.Descricao;
        trilha.Resumo = request.Resumo;
        trilha.ImagemUrl = request.ImagemUrl;
        trilha.PublicadoEm = !trilha.Publicado && request.Publicado ? DateTime.UtcNow : trilha.PublicadoEm;
        trilha.Publicado = request.Publicado;
        trilha.Destaque = request.Destaque;
        trilha.Ordem = request.Ordem;
        trilha.AtualizadoEm = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(trilha);
    }

    public async Task AddConteudoAsync(Guid trilhaId, AddTrilhaConteudoRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _trilhas.GetByIdAsync(trilhaId, true, cancellationToken) ?? throw new AppException("Trilha nao encontrada.", 404);
        _ = await _conteudos.GetByIdAsync(request.ConteudoId, true, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404);
        if (await _trilhas.GetConteudoAsync(trilhaId, request.ConteudoId, cancellationToken) is not null)
        {
            throw new AppException("Conteudo ja esta na trilha.");
        }

        _trilhas.AddConteudo(new TrilhaConteudo { Id = Guid.NewGuid(), TrilhaFormacaoId = trilhaId, ConteudoId = request.ConteudoId, Ordem = request.Ordem, Obrigatorio = request.Obrigatorio, CriadoEm = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReordenarConteudosAsync(Guid trilhaId, ReordenarTrilhaConteudoRequest request, CancellationToken cancellationToken = default)
    {
        foreach (var item in request.Conteudos)
        {
            var trilhaConteudo = await _trilhas.GetConteudoAsync(trilhaId, item.ConteudoId, cancellationToken)
                ?? throw new AppException($"Conteudo {item.ConteudoId} nao encontrado na trilha.", 404);
            trilhaConteudo.Ordem = item.Ordem;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveConteudoAsync(Guid trilhaId, Guid conteudoId, CancellationToken cancellationToken = default)
    {
        var trilhaConteudo = await _trilhas.GetConteudoAsync(trilhaId, conteudoId, cancellationToken)
            ?? throw new AppException("Conteudo nao encontrado na trilha.", 404);
        _trilhas.RemoveConteudo(trilhaConteudo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string BuildSlug(TrilhaRequest request) => string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Titulo) : Slug.From(request.Slug);
    private static TrilhaResponse ToResponse(TrilhaFormacao trilha) =>
        new(trilha.Id, trilha.Titulo, trilha.Slug, trilha.Descricao, trilha.Resumo, trilha.ImagemUrl, trilha.Publicado, trilha.Destaque, trilha.Ordem, trilha.Conteudos.OrderBy(c => c.Ordem).Select(c => new TrilhaConteudoResponse(c.ConteudoId, c.Conteudo.Titulo, c.Ordem, c.Obrigatorio)).ToList());
}
