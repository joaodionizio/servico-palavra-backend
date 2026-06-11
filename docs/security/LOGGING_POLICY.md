# Logging Policy

## Nunca registrar

- Senha.
- Hash de senha.
- Cookie.
- Token CSRF.
- Connection string.
- `INITIAL_ADMIN_PASSWORD`.
- Payload completo de autenticacao.
- Dados pessoais desnecessarios.

## Registrar com cuidado

- Login bem-sucedido com identificador mascarado.
- Login invalido sem revelar se o e-mail existe.
- Acesso negado.
- Tentativa admin indevida.
- Criacao/edicao/publicacao admin.
- Alteracao de plano biblico.
- Falhas criticas com correlation id.

## Mensagens ao usuario

Mensagens publicas devem ser genericas. Stack trace e detalhes tecnicos ficam apenas em logs de servidor, com cuidado para nao incluir segredo.
