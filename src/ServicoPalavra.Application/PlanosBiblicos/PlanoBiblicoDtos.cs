namespace ServicoPalavra.Application.PlanosBiblicos;

public enum ModoAlteracaoPlanoBiblico
{
    ContinuarDeOndeParei = 1,
    RecomecarDoInicio = 2
}

public sealed record CriarPlanoBiblicoRequest(string Nome, int DuracaoAnos, int DuracaoMeses, DateOnly DataInicio);
public sealed record AlterarPlanoBiblicoRequest(int DuracaoAnos, int DuracaoMeses, ModoAlteracaoPlanoBiblico Modo);
public sealed record PlanoBiblicoResponse(Guid Id, string Nome, int DuracaoDias, int DuracaoAnos, int DuracaoMeses, DateOnly DataInicio, DateOnly DataFimPrevista, string Status, int OrdemInicio, int? OrdemFim, string ModoCriacao);
public sealed record PlanoBiblicoDiaResponse(Guid Id, int DiaNumero, int MesNumero, DateOnly? DataPrevista, string? LeiturasTexto, int? SalmoNumero, bool Concluido);
public sealed record PosicaoBiblicaResponse(int UltimaOrdemConcluida, Guid? UltimaBaseBiblicaConcluidaId);
