# Secrets Management

## Variaveis obrigatorias

- `DATABASE_PROVIDER`: `sqlite` local ou `postgresql` producao.
- `ConnectionStrings__DefaultConnection`: fora do repositorio.

## Bootstrap admin

- `INITIAL_ADMIN_EMAIL`
- `INITIAL_ADMIN_PASSWORD`
- `INITIAL_ADMIN_NAME`

Em producao, se essas variaveis nao existirem, nenhum admin inicial e criado. Use senha unica, troque apos o primeiro acesso e remova/rotacione o segredo depois.

## Desenvolvimento

Preferir:

```bash
dotnet user-secrets set "INITIAL_ADMIN_EMAIL" "admin@local"
dotnet user-secrets set "INITIAL_ADMIN_PASSWORD" "<senha-local>"
dotnet user-secrets set "INITIAL_ADMIN_NAME" "Admin Local"
```

`.env` e `.env.local` estao ignorados. `.env.example` contem apenas placeholders.

## Rotacao

- Logout encerra a sessao do cookie atual.
- Rotacionar senha bootstrap apos uso.
- Rotacionar connection string se houver suspeita de vazamento.
