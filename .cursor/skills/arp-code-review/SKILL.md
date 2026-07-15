---
name: arp-code-review
description: >-
  Reviews ARP (.NET 10 + HotChocolate + EF Core) code changes against project
  conventions. Use when the user asks for code review, review de PR, revisão de
  código, review do diff, or to check Modules/ mutations, queries, DataLoaders,
  Context, or migrations before merge. For a dedicated security pass, use
  /review-security (or when the user asks revisão de segurança).
---

# ARP Code Review

Review changes for this GraphQL API only. Follow `.cursor/rules/project.mdc` as the source of truth for stack and patterns.

Do **not** fix issues unless the user explicitly asks after the review.

## Priority (conflicts)

When other skills or subagents disagree with ARP conventions, resolve in this order:

1. `.cursor/rules/project.mdc`
2. This skill (`arp-code-review` + [checklist.md](checklist.md))
3. Installed helpers (optional context only — enrich findings, never override ARP):
   - `modern-csharp-coding-standards` — language/API style
   - `efcore-patterns` — EF Core query/migration hygiene
   - `supabase-postgres-best-practices` — **SQL & performance only** (see below)
4. Cursor built-in `/review-security` — dedicated security subagent (see below)

**ARP wins.** Do **not** flag missing MediatR, repository layer, `ARP.Repo`, Clean Architecture folders, Dapper, or REST controllers when the change already follows project modules + `Context` + HotChocolate.

### `supabase-postgres-best-practices` — SQL/perf only

ARP uses Neon/PostgreSQL via EF Core, **not** Supabase Auth/RLS. When consulting this skill:

- **Use:** indexes, query shape, connection pooling, `EXPLAIN`-minded tips, schema design that helps perf, locking/concurrency that affects query cost
- **Ignore / do not report as findings:** Supabase Auth, Row Level Security (RLS), Supabase client SDK, PostgREST/Realtime, Supabase-specific policies

Map ideas to ARP: LINQ → SQL, `Context` / migrations / unique indexes — not dashboard RLS.

### `/review-security` (Cursor built-in)

When the user asks for **security review**, `/review-security`, or an auth/secrets-focused pass:

1. Follow the Cursor **review-security** skill: launch exactly one `security-review` subagent (`readonly: true`) with the standard prompt (`Full Repository Path`, `Diff: branch changes` or `uncommitted changes`).
2. Do **not** compute the diff yourself before launching the subagent.
3. After it finishes, summarize as that skill requires (table: Severity | Location | Finding).
4. Still apply priority: ARP auth style (Identity + JWT + `refresh_token` cookie) beats generic “rewrite auth” advice.
5. Do **not** auto-fix unless the user asks.

For a **normal** ARP code review, keep a light Auth/security checklist below; do **not** launch `/review-security` unless requested (or the change is clearly auth/secrets-heavy and the user wants a deep pass).

## Workflow

1. **Scope** — Infer what to review:
   - PR / “branch” / “tudo da branch” → `git diff` against the default base (`main`/`master`)
   - “uncommitted” / “local” / “staged” → working tree / staged only
   - Specific files or pastes → those files only
   - Security-only request → jump to `/review-security` section above
2. **Read** — Inspect the diff and enough surrounding context (module siblings, `Context`, `Program.cs` registration).
3. **Check** — Apply the checklist below (and [checklist.md](checklist.md) for severity examples). For DB-touching changes, enrich with **SQL/perf** from `supabase-postgres-best-practices` (indexes, N+1 → SQL, filter/sort pushdown).
4. **Report** — Use the output format. Sort findings by severity (Critical → Nice). Skip empty severity sections.
5. **Stop** — Do not auto-commit, push, or rewrite code.

If the diff is empty, say so in one sentence and stop.

## Checklist (must cover)

### Architecture / modules
- [ ] New feature under `ARP/Modules/{Feature}/` with `*ModuleConfig`, Query/Mutation as needed, `Types/` records
- [ ] Wired in `Program.cs` (`.AddXQueriesAndMutations()` / DI)
- [ ] Entity in `ARP.Entity`; `DbSet` + relationships in `ARP.Infra/Context`
- [ ] Migration present when model changed (`ARP.Infra` + startup `ARP`)
- [ ] No new use of empty `ARP.Repo`; no NestJS/TypeORM/REST controllers for domain CRUD

### GraphQL / HotChocolate
- [ ] Lists with `[UsePaging]/[UseFiltering]/[UseSorting]/[UseProjection]` return `IQueryable` / `AsQueryable()` — **not** `ToListAsync()` first
- [ ] Related collections use `BatchDataLoader` + field resolver — not per-parent `DbSet` queries (N+1)
- [ ] DataLoaders use `IDbContextFactory<Context>` + `await using` short-lived context
- [ ] Mutations/resolvers inject `[Service] Context` (not a second ORM)
- [ ] Inputs/payloads are `record`s with `[GraphQLDescription]` where appropriate

### Data / EF Core / Postgres (SQL & perf)
- [ ] Soft-deletable (`Base`) entities: delete via `DeletedAt = …`, not `Remove()`
- [ ] No redundant `DeletedAt == null` filters on filtered entities
- [ ] CPF/CNPJ validated/normalized via `CpfHelper` / `CnpjHelper`
- [ ] Dates in UTC (`DateTime.UtcNow` / `DateTimeHelper`)
- [ ] Prefer LINQ; no new `FromSqlRaw` unless clearly justified
- [ ] Transactions: rollback + rethrow on failure (no empty `catch`)
- [ ] **SQL/perf:** filters/sorts stay on `IQueryable`; hot filters have sensible indexes/uniques in `OnModelCreating`/migrations; avoid loading wide graphs when a projection/loader suffices (use postgres skill for **query/index/pooling** tips only — not RLS/Supabase Auth)

### Auth / security (light pass)
- [ ] Sensitive mutations/queries consider `[Authorize]` vs `[AllowAnonymous]` consistently with Auth patterns
- [ ] No secrets committed (JWT, connection strings, passwords) in new/changed files
- [ ] Cookies/JWT handling does not weaken existing httpOnly / Secure / SameSite approach without reason
- [ ] Deep security audit → run `/review-security` when the user asks

### Quality
- [ ] Prefer `async`/`await` + `CancellationToken` on IO paths
- [ ] `ILogger<T>` used; no new Critical “START SQL” noise
- [ ] Public service/utils methods have XML docs (`///`)
- [ ] Errors: `ArgumentException` / `GraphQLException` / Auth payloads match existing style for that area

## Output format

Respond in **pt-BR**. Start with a one-line veredito.

```markdown
## Veredito
[Aprovar / Aprovar com ressalvas / Solicitar mudanças] — [1 frase]

## Achados

| Severidade | Local | Problema | Sugestão |
|------------|-------|----------|----------|
| Critical / Warning / Suggestion / Nice | `path:line` | … | … |

## O que está bom
- [bullets curtos do que segue o padrão do repo]

## Perguntas (se houver)
- [só dúvidas que bloqueiam o review]
```

**Severities:**
- 🔴 **Critical** — bug, data loss, security, broken GraphQL middleware, hard delete on `Base`, secrets
- 🟡 **Warning** — N+1, missing module registration/migration, wrong soft-delete, ToList before paging, missing index on hot filter
- 🔵 **Suggestion** — better layering (`ARP.Service`), logging, naming, Authorization gaps, SQL/perf polish
- 🟢 **Nice** — polish only

If no issues: one-line veredito + short “O que está ok”. No empty tables.

## Out of scope

- Do not invent NestJS/TypeORM findings.
- Do not fail the review solely for missing tests unless the user asked for test coverage.
- Do not require rewrites of legacy `Pessoa` sample quirks unless the change expands them.
- Do not treat `efcore-patterns` / `modern-csharp-coding-standards` advice that conflicts with ARP (repos, NoTracking-by-default vs existing patterns, Result-type mandates) as merge blockers.
- Do **not** demand Supabase RLS, Supabase Auth, or Supabase client patterns from `supabase-postgres-best-practices`.