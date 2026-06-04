import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerInterceptor implements HttpInterceptor {
  constructor(private messageService: MessageService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(error => {
        if (!(error instanceof HttpErrorResponse)) {
          return throwError(() => error);
        }

        if (error.status === 401) {
          return throwError(() => error);
        }

        if (error.status === 403) {
          const detail = this.extractDetail(error) ?? 'Bạn không có quyền thực hiện thao tác này.';
          this.messageService.add({ severity: 'error', summary: 'Không có quyền', detail });
          return throwError(() => error);
        }

        if (error.status >= 500) {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi Server',
            detail: 'Lỗi hệ thống. Vui lòng liên hệ quản trị viên.',
          });
          return throwError(() => error);
        }

        return throwError(() => error);
      }),
    );
  }

  private extractDetail(error: HttpErrorResponse): string | null {
    if (typeof error.error === 'string') return error.error;
    if (error.error?.detail) return error.error.detail;
    if (error.error?.title) return error.error.title;
    return null;
  }
}
