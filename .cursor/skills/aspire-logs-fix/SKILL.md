---
name: aspire-logs-fix
description: Đọc và phân tích log từ .NET Aspire Dashboard đang chạy. Dùng khi user gặp lỗi, muốn xem log, hoặc nói về Aspire/log/build error/runtime error.
---

# Aspire Logs

## Đọc log

```bash
aspire logs --format Json
```

Nếu `aspire` not found:
```bash
dotnet tool install -g Aspire.Cli
```

## Output format

JSON với các trường:
- `resourceName` - tên service (webfrontend, webapi, etc.)
- `content` - nội dung log
- `isError` - true = lỗi

## Lọc nhanh

```bash
# Chỉ lỗi
aspire logs --format Json 2>&1 | Select-Object -ExpandProperty logs | Where-Object { $_.isError }

# Theo service
aspire logs webfrontend --format Json

# Realtime stream
aspire logs --follow --format Json
```

## Cách đọc

Đọc từ trên xuống. Error block thường gồm:
1. Dòng `X [ERROR]` - mã lỗi (TS2307, TS2345, etc.)
2. Dòng mô tả lỗi
3. Dòng `file:line:` - vị trí file gây lỗi

Map `file:line` đến file thực tế, đọc file, phân tích và fix.
