# Story E1-BE-01 — Recuperação segura de senha

Status: backlog  
Prioridade: P0

## Objetivo
Permitir que uma pessoa recupere o acesso à conta com um link temporário enviado por e-mail, sem expor se um endereço está cadastrado e sem adicionar custo recorrente obrigatório.

## Decisões
- usar os token providers do ASP.NET Core Identity; não criar tabela própria de tokens;
- não criar migration para esta feature;
- enviar link para o frontend contendo identificador opaco/GUID do usuário e token URL-safe;
- transporte de e-mail deve ficar atrás de uma abstração e ser configurado por variáveis de ambiente;
- produção deve aceitar SMTP compatível com opção gratuita/sem custo recorrente; nenhuma credencial entra no repositório;
- testes usam sender fake/in-memory e não enviam e-mail real.

## Escopo
- DTOs para solicitar recuperação e redefinir senha;
- métodos no `IAuthService`/`AuthService` para gerar e validar reset;
- endpoints públicos em `/api/auth`;
- sender abstrato para e-mail de recuperação;
- implementação de infraestrutura configurável por ambiente;
- link baseado em `FRONTEND_BASE_URL` ou configuração equivalente;
- rate limiting específico para solicitação/reset;
- testes de integração e segurança.

## Endpoints propostos

### POST `/api/auth/password/forgot`
Body:
```json
{ "email": "pessoa@example.com" }
```

Resposta pública, sempre que a requisição for sintaticamente válida:
```json
{ "success": true, "message": "Se existir uma conta com esse e-mail, enviaremos as instruções de recuperação." }
```

Nunca revelar se usuário existe, está inativo ou se o e-mail foi enviado.

### POST `/api/auth/password/reset`
Body:
```json
{
  "userId": "guid",
  "token": "token-url-safe",
  "novaSenha": "..."
}
```

Sucesso invalida naturalmente o token anterior conforme as regras do Identity/security stamp. Token inválido/expirado retorna mensagem genérica apropriada, sem detalhes internos.

## Segurança
1. Solicitação de recuperação não permite enumeração de usuários por status, mensagem ou estrutura da resposta.
2. Token é gerado por `UserManager.GeneratePasswordResetTokenAsync` e nunca persistido em texto puro pela aplicação.
3. Token enviado em URL é codificado com Base64Url/encoding seguro e corretamente decodificado no reset.
4. O link não inclui senha, cookie ou dado secreto adicional.
5. Logs não registram token, senha, cookie, SMTP password ou link completo de recuperação.
6. Rate limiting protege os dois endpoints; solicitação deve ter limite mais restritivo que endpoints gerais.
7. Reset usa `UserManager.ResetPasswordAsync`; não altera hash manualmente.
8. Usuário inativo não recebe fluxo utilizável, mas a resposta pública da solicitação continua indistinguível.
9. Expiração do token fica explícita e curta (preferência 30–60 minutos), sem reduzir inadvertidamente segurança de outros token providers.
10. CSRF continua aplicado conforme política atual para writes; a natureza pública do endpoint não remove antiforgery automaticamente.

## E-mail
Conteúdo mínimo:
- identidade Serviço da Palavra;
- aviso de que houve solicitação de redefinição;
- botão/link HTTPS para o frontend;
- validade aproximada do link;
- orientação para ignorar se não foi a pessoa;
- nunca incluir a nova senha.

Configuração esperada via env/config, sem valores reais versionados:
- `FRONTEND_BASE_URL`
- host/porta SMTP ou configuração equivalente do transport escolhido;
- usuário/senha/credencial via secret;
- remetente e nome de exibição.

## Política de senha
A implementação deve usar uma única política coerente entre cadastro e reset. O código atual possui uma restrição manual de 6–8 caracteres no cadastro que diverge das opções do Identity; essa inconsistência deve ser explicitamente resolvida ou registrada como bloqueio de segurança antes de concluir esta story. Não introduzir uma terceira política diferente no reset.

## Critérios de aceitação
1. Solicitar reset para e-mail existente e inexistente produz resposta pública indistinguível.
2. Para conta válida/ativa, sender recebe exatamente um link de reset corretamente formado.
3. Link contém `userId` e token URL-safe, apontando para rota configurada do frontend.
4. Token válido permite trocar senha e autenticar com a nova senha.
5. Senha antiga deixa de autenticar após reset bem-sucedido.
6. Token inválido, alterado ou expirado não redefine senha.
7. Reutilização do mesmo token após reset bem-sucedido falha.
8. Rate limiting é coberto por teste ou configuração verificável.
9. Nenhum secret/token aparece em logs/test snapshots/documentação.
10. `bash tools/verify.sh` permanece verde.

## Fora de escopo
- confirmação de e-mail de cadastro;
- MFA;
- alteração de senha por usuário já autenticado;
- mudança de provedor de hospedagem;
- serviço pago de e-mail obrigatório.

## Arquivos/áreas prováveis
- `src/ServicoPalavra.Api/Controllers/AuthController.cs`
- `src/ServicoPalavra.Api/Program.cs`
- `src/ServicoPalavra.Application/Auth/*`
- abstração de envio em Application
- implementação/configuração em Infrastructure
- `tests/ServicoPalavra.IntegrationTests/SecurityTests.cs` ou suíte específica de auth
- `.env.example` e docs de deploy sem secrets reais

## Definition of Done
- critérios 1–10 atendidos;
- testes automatizados cobrindo happy path e abuso;
- verify verde;
- documentação de configuração sem credenciais;
- story em `review` e code review independente aprovado.
