# Matriz De Seguranca Dos Endpoints

Esta matriz resume o acesso esperado dos endpoints atuais. Para requisicoes `POST`, `PUT`, `PATCH` e `DELETE`, a API exige CSRF via header `X-CSRF-TOKEN`, alem do cookie de sessao quando o endpoint for autenticado.

| Endpoint | Acesso | Usa usuario autenticado do backend? | Risco de acesso cruzado | Cobertura atual |
|---|---|---:|---|---|
| `GET /` | Publico | Nao | Baixo | Pendente de validacao dedicada. |
| `GET /health` | Publico | Nao | Baixo | Usado em smoke tests manuais. |
| `GET /api/auth/csrf` | Publico | Nao | Baixo | Coberto indiretamente em testes CSRF. |
| `POST /api/auth/register` | Publico com CSRF | Nao | Baixo | Cobertura de cadastro/login parcial. |
| `POST /api/auth/login` | Publico com CSRF | Nao | Baixo | Testes de login valido/invalido/lockout. |
| `POST /api/auth/logout` | Autenticado com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `GET /api/auth/me` | Autenticado | Sim | Medio | Rota protegida coberta; acesso cruzado nao aplicavel. |
| `GET /api/categorias` | Publico | Nao | Baixo | Pendente de teste dedicado. |
| `POST /api/admin/categorias` | Admin com CSRF | Sim | Baixo | Teste usuario comum bloqueado/admin aceito. |
| `PUT /api/admin/categorias/{id}` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `GET /api/conteudos` | Publico | Nao | Baixo | Pendente de teste dedicado. |
| `GET /api/conteudos/{id}` | Publico | Nao | Baixo | Pendente de teste dedicado. |
| `POST /api/admin/conteudos` | Admin com CSRF | Sim | Baixo | Teste de URL maliciosa e admin parcial. |
| `PUT /api/admin/conteudos/{id}` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `PATCH /api/admin/conteudos/{id}/publicar` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `PATCH /api/admin/conteudos/{id}/despublicar` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `GET /api/trilhas` | Publico | Nao | Baixo | Pendente de teste dedicado. |
| `GET /api/trilhas/{id}` | Publico | Nao | Baixo | Pendente de teste dedicado. |
| `POST /api/admin/trilhas` | Admin com CSRF | Sim | Baixo | Admin parcial coberto. |
| `PUT /api/admin/trilhas/{id}` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `POST /api/admin/trilhas/{id}/conteudos` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `PUT /api/admin/trilhas/{id}/conteudos/ordem` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `DELETE /api/admin/trilhas/{id}/conteudos/{conteudoId}` | Admin com CSRF | Sim | Baixo | Pendente de teste dedicado. |
| `GET /api/favoritos` | Autenticado | Sim | Medio | Pendente de teste UsuarioA x UsuarioB dedicado. |
| `POST /api/conteudos/{id}/favoritar` | Autenticado com CSRF | Sim | Medio | Pendente de teste UsuarioA x UsuarioB dedicado. |
| `DELETE /api/conteudos/{id}/favoritar` | Autenticado com CSRF | Sim | Medio | Pendente de teste UsuarioA x UsuarioB dedicado. |
| `POST /api/conteudos/{id}/concluir` | Autenticado com CSRF | Sim | Medio | Pendente de teste UsuarioA x UsuarioB dedicado. |
| `GET /api/dashboard/me` | Autenticado | Sim | Medio | Pendente de teste UsuarioA x UsuarioB dedicado. |
| `GET /api/planos-biblicos/ativo` | Autenticado | Sim | Medio | Coberto indiretamente. |
| `GET /api/planos-biblicos/me/ativo` | Autenticado | Sim | Medio | Coberto em testes de plano. |
| `GET /api/planos-biblicos/me/historico` | Autenticado | Sim | Medio | Coberto em fluxo de historico. |
| `POST /api/planos-biblicos` | Autenticado com CSRF | Sim | Alto se UsuarioId viesse do frontend | Testado: nao permite dois planos ativos. |
| `POST /api/planos-biblicos/alterar` | Autenticado com CSRF | Sim | Alto se estado parcial nao fosse transacional | Testado: continuar/recomecar e rollback. |
| `GET /api/planos-biblicos/{id}` | Autenticado | Sim | Alto | Testado: usuario nao le plano de outro por GUID. |
| `GET /api/planos-biblicos/{id}/dias` | Autenticado | Sim | Alto | Coberto nos fluxos de plano; teste cruzado especifico pendente. |
| `GET /api/planos-biblicos/progresso/posicao-atual` | Autenticado | Sim | Medio | Coberto indiretamente. |
| `POST /api/planos-biblicos/dias/{diaId}/concluir` | Autenticado com CSRF | Sim | Alto | Testado: usuario nao conclui dia de outro. |
| `POST /api/planos-biblicos/dias/{diaId}/desmarcar` | Autenticado com CSRF | Sim | Alto | Testado: usuario nao desmarca dia de outro. |

## Observacoes

- O backend deve continuar derivando `UsuarioId` exclusivamente da sessao autenticada.
- Endpoints publicos devem retornar apenas conteudos publicados ou informacoes operacionais seguras.
- Endpoints admin usam role `Admin`; regras especificas para `Coordenador` seguem pendentes de validacao.
- Secrets, connection strings reais, cookies e tokens nao devem ser versionados.
