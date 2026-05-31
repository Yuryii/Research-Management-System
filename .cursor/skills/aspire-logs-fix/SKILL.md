---
name: aspire-logs-fix
description: Read and analyze logs from .NET Aspire Dashboard. Use when user encounters errors, wants to view logs, or mentions Aspire/log/build error/runtime error.
---

# Aspire Logs

## Read all logs

```bash
# All logs from every service (backend + frontend + database + etc.)
aspire logs --format Json

# Realtime stream all logs
aspire logs --follow --format Json
```

## Quick filters

```bash
# Get content lines containing errors (with 1 preceding line for service name context)
aspire logs --format Json 2>&1 | Select-String -Pattern "ERROR" -Context 1,0 | Select-Object -Last 40

# Get content lines containing errors, last 20 lines
aspire logs --format Json 2>&1 | Select-String -Pattern "ERROR" | Select-Object -Last 20

# Per specific service
aspire logs webfrontend --format Json

# Get all error lines with context (do NOT use -ExpandProperty - each log entry is a separate object in the logs array, not a property)
aspire logs --format Json 2>&1 | Select-String -Pattern "isError" -Context 0,1 | Select-Object -Last 60
```

## Important notes

### ANSI Escape Codes
Aspire logs contain ANSI escape codes in the `content` field, for example:
```
\u001B[31mX \u001B[41;31m[\u001B[41;97mERROR\u001B[41;31m]\u001B[0m
```
Therefore:
- **DO NOT** use `Select-String -Pattern "\[ERROR\]"` — the ANSI codes break literal bracket matching. Use `Select-String -Pattern "ERROR"` (without brackets) instead.
- **DO NOT** use `Select-Object -ExpandProperty logs` — each log line is a separate entry in the `logs` array, not a property on a single object.

### JSON output structure
```json
{
  "logs": [
    { "resourceName": "webfrontend", "content": "X [ERROR] TS2724: ...", "isError": true },
    { "resourceName": "webfrontend", "content": "src/app/...", "isError": true }
  ]
}
```
Each log line is its own object inside the `logs` array. To view errors, read the `content` field for the message — do not just look at `isError: true`.

## How to read

Read from top to bottom. Error blocks typically include:
1. Line with `X [ERROR]` — error code (TS2307, TS2345, NG1010, etc.)
2. Line with error description
3. Line with `file:line:` — source file location

Map `file:line` to the actual source file, read it, analyze, and fix.
