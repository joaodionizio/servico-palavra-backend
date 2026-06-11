using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Application.Conteudos;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Favoritos;

public sealed class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _favoritos;
    private readonly IConteudoRepository _conteudos;
    private readonly IUnitOfWork _unitOfWork;

    public FavoritoService(IFavoritoRepository favoritos, IConteudoRepository conteudos, IUnitOfWork unitOfWork)
    {
        _favoritos = favoritos;
        _conteudos = conteudos;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ConteudoResponse>> ListAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        (await _favoritos.ListByUsuarioAsync(usuarioId, cancellationToken)).Select(f => new ConteudoResponse(f.Conteudo.Id, f.Conteudo.Titulo, f.Conteudo.Slug, f.Conteudo.Descricao, f.Conteudo.Resumo, f.Conteudo.Tipo, f.Conteudo.Origem, f.Conteudo.Url, f.Conteudo.UrlThumbnail, f.Conteudo.DuracaoMinutos, f.Conteudo.CategoriaConteudoId, f.Conteudo.CategoriaConteudo.Nome, f.Conteudo.Publicado, f.Conteudo.Destaque, f.Conteudo.Ordem, f.Conteudo.PublicadoEm)).ToList();

    public async Task FavoritarAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default)
    {
        _ = await _conteudos.GetByIdAsync(conteudoId, false, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404);
        if (await _favoritos.GetAsync(usuarioId, conteudoId, cancellationToken) is not null)
        {
            return;
        }

        _favoritos.Add(new Favorito { Id = Guid.NewGuid(), UsuarioId = usuarioId, ConteudoId = conteudoId, CriadoEm = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default)
    {
        var favorito = await _favoritos.GetAsync(usuarioId, conteudoId, cancellationToken);
        if (favorito is null)
        {
            return;
        }

        _favoritos.Remove(favorito);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
