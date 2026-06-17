import { Injectable, computed, inject, signal } from '@angular/core';
import { NotificationsClient, NotificationDto } from '../../web-api-client';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly client = inject(NotificationsClient);

  private readonly _unreadCount = signal(0);
  private readonly _recent = signal<NotificationDto[]>([]);
  private readonly _loading = signal(false);

  readonly unreadCount = this._unreadCount.asReadonly();
  readonly recent = this._recent.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly hasUnread = computed(() => this._unreadCount() > 0);

  loadUnreadCount(): void {
    this.client.getUnreadCount().subscribe({
      next: (count) => this._unreadCount.set(count ?? 0),
      error: () => this._unreadCount.set(0),
    });
  }

  loadRecent(limit = 10): void {
    this._loading.set(true);
    this.client.getMyNotifications(1, limit).subscribe({
      next: (result) => {
        this._recent.set(result?.items ?? []);
        this._loading.set(false);
      },
      error: () => {
        this._recent.set([]);
        this._loading.set(false);
      },
    });
  }

  refresh(): void {
    this.loadUnreadCount();
    this.loadRecent();
  }

  markAsRead(id: string): void {
    this.client.markAsRead(id).subscribe({
      next: () => {
        this._recent.update((items) => {
          for (const n of items) {
            if (n.id === id && !n.isRead) {
              n.isRead = true;
              n.readAt = new Date();
            }
          }
          return [...items];
        });
        const current = this._unreadCount();
        if (current > 0) {
          this._unreadCount.set(current - 1);
        }
      },
    });
  }

  markAllAsRead(): void {
    this.client.markAllAsRead().subscribe({
      next: () => {
        this._recent.update((items) => {
          const now = new Date();
          for (const n of items) {
            if (!n.isRead) {
              n.isRead = true;
              n.readAt = now;
            }
          }
          return [...items];
        });
        this._unreadCount.set(0);
      },
    });
  }

  static typeLabel(type: number): string {
    switch (type) {
      case 1:
        return 'Trả hồ sơ';
      case 2:
        return 'Chuyển tiếp';
      case 3:
        return 'Phê duyệt';
      default:
        return 'Thông báo';
    }
  }
}
