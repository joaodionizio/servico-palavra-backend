using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.PlanosBiblicos;

public interface IBiblePlanGeneratorService
{
    Task<PlanoBiblicoUsuario> GenerateAsync(
        Guid usuarioId,
        string nome,
        int duracaoAnos,
        int duracaoMeses,
        DateOnly dataInicio,
        ModoCriacaoPlanoBiblico modo,
        int ordemInicio,
        Guid? planoOrigemId,
        CancellationToken cancellationToken = default);
}
