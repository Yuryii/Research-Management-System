import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

export interface SwaggerException {
  message: string;
  status: number;
  response: string;
  headers: { [key: string]: any };
  result: any;
  isSwaggerException?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  private readonly messageService = inject(MessageService);

  extractMessage(error: unknown): string {
    if (this.isSwaggerException(error)) {
      return this.extractFromSwaggerException(error);
    }

    if (this.isHttpErrorResponse(error)) {
      return this.extractFromHttpErrorResponse(error);
    }

    if (typeof error === 'string') return error;

    if (error instanceof Error) return error.message;

    return 'Đã có lỗi xảy ra. Vui lòng thử lại.';
  }

  private extractFromSwaggerException(error: SwaggerException): string {
    if (error.result?.detail) return error.result.detail;
    if (error.result?.title) return error.result.title;
    if (typeof error.result === 'string') return error.result;
    return error.message;
  }

  private extractFromHttpErrorResponse(error: any): string {
    if (error.error?.detail) return error.error.detail;
    if (error.error?.title) return error.error.title;
    if (typeof error.error === 'string') return error.error;
    return error.message || `Lỗi HTTP ${error.status}`;
  }

  private isSwaggerException(error: unknown): error is SwaggerException {
    return (error as SwaggerException)?.isSwaggerException === true;
  }

  private isHttpErrorResponse(error: unknown): boolean {
    return error !== null && typeof error === 'object' && 'status' in error && 'error' in error;
  }

  showError(detail: string, summary = 'Lỗi'): void {
    this.messageService.add({ severity: 'error', summary, detail });
  }

  showSuccess(detail: string, summary = 'Thành công'): void {
    this.messageService.add({ severity: 'success', summary, detail });
  }

  showWarning(detail: string, summary = 'Cảnh báo'): void {
    this.messageService.add({ severity: 'warn', summary, detail });
  }
}
