# Identity Migration

Data: 2026-06-11

## Estado anterior

- Autenticacao artesanal baseada em `Usuario`, `Perfil`, BCrypt e JWT Bearer.
- Tabela `Perfis` usada apenas como autorizacao/role.
- `Usuario.SenhaHash` armazenado pela aplicacao.
- Endpoints admin protegidos por role no JWT.
- Plano biblico, favoritos e progresso ja derivam `UsuarioId` do usuario autenticado.

## Decisao tomada

Migrar para ASP.NET Core Identity com:

- `ApplicationUser : IdentityUser<Guid>`;
- `IdentityRole<Guid>`;
- cookies de aplicacao para navegadores;
- antiforgery para operacoes de escrita autenticadas por cookie;
- lockout e password hasher do Identity;
- roles `Usuario`, `Admin`, `Coordenador`.

## Perfil e roles

`Perfil` era usado como autorizacao. A autorizacao passa a ser role do Identity. A tabela antiga pode existir em migrations antigas, mas nao deve ser fonte de autorizacao.

## Migracao de dados

Como ainda nao ha producao real, a baseline atual ja nasce com Identity. Se houver banco com dados criados antes desta alteracao, migre `Perfil` para `AspNetUserRoles` antes de remover a dependencia antiga e force reset de senha, pois hash BCrypt antigo nao e compatibilizado com o formato do Identity.

## Segredos e admin inicial

Admin inicial so pode ser criado quando `INITIAL_ADMIN_EMAIL`, `INITIAL_ADMIN_PASSWORD` e `INITIAL_ADMIN_NAME` forem informados. Nenhuma senha real e versionada.

## Pendencias conscientes

- Validar baseline/migrations em PostgreSQL/Neon com connection string real autorizada.
- Definir politica final de SameSite conforme dominio Vercel/Render ou proxy/rewrite.
