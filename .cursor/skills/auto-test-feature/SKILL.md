---
name: auto-test-feature
description: End-to-end test a feature using browser automation, API calls, Aspire logs, and SQL queries. Use when the user asks to test, verify, or validate a feature end-to-end, or when the user says "use the auto-test-feature skill".
---

# Auto-Test Feature

## Context

- App URL: `https://rms.dev.localhost:17114`
- DB connection: `Server=(localdb)\\mssqllocaldb;Database=RMSDbb;Trusted_Connection=True;MultipleActiveResultSets=true`
- DB queries: use `sqlcmd` (see `sql-query-workflow` rule) or `scripts/mssql-mcp-launcher/start.js`
- Logs: use `aspire-logs-fix` skill after every test run

## Determine Test Types

From the feature description, pick the applicable types and run in sequence:

| Feature involves | Test type | Tool |
|---|---|---|
| Backend logic, commands/queries | Functional | MediatR dispatch |
| UI interactions | Browser E2E | `cursor-ide-browser` MCP |
| API endpoints | API test | `web-api-client.ts` |
| Data state changes | DB verification | `sqlcmd` |

Always end with log verification.

---

## Phase 1: Functional Tests

Use the `TestApp` static helper from `tests/Application.FunctionalTests/Infrastructure/TestApp.cs`.

**Pattern:**

```csharp
public class [Feature]Tests : TestBase
{
    [Test]
    public async Task Should_[behavior]()
    {
        await TestApp.RunAsAdministratorAsync();          // or RunAsDefaultUserAsync(), RunAsUserAsync(un, pw, roles)
        await TestApp.AddAsync(seedEntity);               // optional seed
        var result = await TestApp.SendAsync(command);   // dispatch MediatR command/query
        result.ShouldBe(expected);                        // Shouldly assertions
        var entity = await TestApp.FindAsync<Entity>(id); // verify DB state
    }
}
```

**Key helpers:**
- `TestApp.SendAsync<T>(IRequest<T>)` / `TestApp.SendAsync(IBaseRequest)` — dispatch MediatR
- `TestApp.RunAsAdministratorAsync()` / `RunAsDefaultUserAsync()` / `RunAsUserAsync(name, pw, roles[])` — set auth principal
- `TestApp.AddAsync<T>(T entity)` — seed DB
- `TestApp.FindAsync<T>(keyValues)` — read DB entity
- `TestApp.ExecuteDbContextAsync(ctx => ...)` — raw EF Core access
- `Should.ThrowAsync<T>(() => ...)` — assert exceptions

Create the test file under `tests/Application.FunctionalTests/Application/[Feature]/`.

---

## Phase 2: Browser E2E

Use the `cursor-ide-browser` MCP server.

**Sequence:**

1. `browser_tabs` with action `list` — check for existing tabs
2. `browser_navigate` to the app URL
3. `browser_snapshot` — read page structure
4. `browser_lock` with action `lock` — lock the tab
5. Interactions: `browser_click`, `browser_fill`, `browser_type`, `browser_select_option`, `browser_press_key`
6. `browser_take_screenshot` — visual checkpoint at key steps
7. `browser_lock` with action `unlock` when done

**Important:**
- Lock/unlock workflow: `browser_navigate` FIRST, THEN `browser_lock({ action: "lock" })`, then interact, then `browser_lock({ action: "unlock" })`
- Do NOT use CDP `Input.*` methods — use dedicated tools instead
- If login is required, use `browser_snapshot` to check for login page elements first
- Iframe content is not accessible — only elements outside iframes can be interacted with

---

## Phase 3: API Tests

Use the nswag-generated client from `src/Web/FE/src/app/web-api-client.ts`.

```typescript
import { ApplicationsClient, StepsClient } from '../../../web-api-client';

const client = new ApplicationsClient(this.http, this.baseUrl);
client.getApplications().subscribe(data => {
    expect(data.items.length).toBeGreaterThan(0);
});
```

Never use `HttpClient` directly for calls that have a generated client. Never edit `web-api-client.ts` manually.

---

## Phase 4: DB Verification

Use `sqlcmd` per the `sql-query-workflow` rule.

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -d "RMSDbb" -Q "SELECT * FROM Applications WHERE Id = '...';"
```

If `sqlcmd` is unavailable, use `scripts/mssql-mcp-launcher/start.js`.

---

## Phase 5: Log Verification

Read `aspire-logs-fix` skill and follow its instructions:

```bash
aspire logs --format Json 2>&1 | Select-String -Pattern "ERROR" -Context 1,0 | Select-Object -Last 40
```

Report errors. Fix them before concluding.

---

## Iteration Loop

If errors are found:
1. Fix the code (backend or frontend)
2. Rebuild (if BE changed): `dotnet build src/AppHost/`
3. Re-run the relevant test phase
4. Repeat until all pass

---

## Reporting

Report:
- What passed (functional / browser / API / DB)
- What failed with the error
- What was fixed
- Any remaining issues
