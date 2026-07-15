# ARP review — severity examples

Use these as anchors when classifying findings. Paths are illustrative.

## Critical

| Pattern | Why |
|---------|-----|
| `context.Empresas.Remove(entity)` on `Base` | Soft-delete required (`DeletedAt`) |
| Connection string / `JWT_KEY` committed | Secret leak |
| Mutation creates entity without `SaveChangesAsync` / wrong DbSet | Data not persisted or wrong table |
| Auth endpoint accidentally drops cookie Secure/httpOnly in production path | Session theft risk |

## Warning

| Pattern | Why |
|---------|-----|
| `ToListAsync()` then `[UsePaging]/`UseFiltering`` | Middleware cannot push down to SQL |
| `db.Setores.Where(s => s.EmpresaId == parent.Id)` in field resolver | N+1; use `BatchDataLoader` |
| New module files but missing `Program.cs` `.Add…QueriesAndMutations()` | Feature not exposed |
| Model change without EF migration under `ARP.Infra/Migrations` | Runtime/schema drift |
| DataLoader injects scoped `Context` instead of `IDbContextFactory` | Pooling / lifetime bugs |
| Skip `CpfHelper`/`CnpjHelper` on create/update | Invalid or unnormalized documents |
| Hot filter/join column with no index/unique in migrations | SQL/perf (postgres skill — ignore RLS/Supabase Auth) |

## Suggestion

| Pattern | Why |
|---------|-----|
| Complex rules only in Mutation (100+ lines) | Prefer `ARP.Service` |
| New endpoints without `[Authorize]` while siblings are protected | Inconsistent auth surface |
| Swallow exception and return success | Hides failures |
| `Include` + DataLoader for same navigation | Duplicate work; prefer loader |

## Nice

| Pattern | Why |
|---------|-----|
| Missing `[GraphQLDescription]` on new input field | Docs/UX in GraphiQL |
| `LogLevel.Information` without template args | Harder to filter logs |
| Public util without `///` XML doc | Team convention |

## Good patterns (do not flag)

- Returning `context.X.AsQueryable()` from paged queries
- Soft delete: `entity.DeletedAt = DateTime.UtcNow`
- `EmpresaResolvers` + `SetoresByEmpresaIdDataLoader` style batching
- Auth login/register returning `*Payload` records with `Success`/`Message`
- `ExecuteDeleteAsync` in cleanup jobs for non-`Base` bulk purge (e.g. refresh tokens)
- Missing Supabase RLS / Supabase Auth (ARP is EF + Identity + Neon — not in scope)

## Security deep pass

Use Cursor built-in `/review-security` (security-review subagent) only when the user asks — not as part of every ARP review.
)
