---
name: aspire-logs-fix
description: Đọc và phân tích log từ .NET Aspire Dashboard đang chạy. Dùng khi user gặp lỗi, muốn xem log, hoặc nói về Aspire/log/build error/runtime error.
---

# Aspire Logs

## Đọc tất cả log

```bash
# Tất cả log từ mọi service (backend + frontend + database + etc.)
aspire logs --format Json

# Realtime stream toàn bộ log
aspire logs --follow --format Json
```

## Lọc nhanh

```bash
# Chỉ lỗi
aspire logs --format Json 2>&1 | Select-Object -ExpandProperty logs | Where-Object { $_.isError }

# Theo service cụ thể
aspire logs webfrontend --format Json
```

## Cách đọc

Đọc từ trên xuống. Error block thường gồm:
1. Dòng `X [ERROR]` - mã lỗi (TS2307, TS2345, etc.)
2. Dòng mô tả lỗi
3. Dòng `file:line:` - vị trí file gây lỗi

Map `file:line` đến file thực tế, đọc file, phân tích và fix.
