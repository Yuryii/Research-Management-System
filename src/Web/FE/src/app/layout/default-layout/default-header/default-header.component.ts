import { AuthService } from './../../../../api-authorization/auth.service';
import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, input, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import {
  AvatarComponent,
  BadgeComponent,
  BreadcrumbRouterComponent,
  ColorModeService,
  ContainerComponent,
  DropdownComponent,
  DropdownDividerDirective,
  DropdownHeaderDirective,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  HeaderComponent,
  HeaderNavComponent,
  HeaderTogglerDirective,
  NavItemComponent,
  NavLinkDirective,
  SidebarToggleDirective,
} from '@coreui/angular';

import { IconDirective } from '@coreui/icons-angular';
import { NotificationService } from '../../../shared/services/notification.service';

@Component({
  selector: 'app-default-header',
  templateUrl: './default-header.component.html',
  styleUrls: ['./default-header.component.scss'],
  imports: [
    ContainerComponent,
    HeaderTogglerDirective,
    SidebarToggleDirective,
    IconDirective,
    HeaderNavComponent,
    NavItemComponent,
    NavLinkDirective,
    RouterLink,
    RouterLinkActive,
    NgTemplateOutlet,
    BreadcrumbRouterComponent,
    DropdownComponent,
    DropdownToggleDirective,
    AvatarComponent,
    DropdownMenuDirective,
    DropdownHeaderDirective,
    DropdownItemDirective,
    BadgeComponent,
    DropdownDividerDirective,
  ],
})
export class DefaultHeaderComponent extends HeaderComponent implements OnInit {
  private readonly AuthService = inject(AuthService);
  private readonly RouterLink = inject(Router);
  private readonly notifications = inject(NotificationService);

  logout = () => {
    this.AuthService.logout().subscribe(() => {
      this.RouterLink.navigate(['/login']);
    });
  };

  readonly #colorModeService = inject(ColorModeService);
  readonly colorMode = this.#colorModeService.colorMode;

  readonly colorModes = [
    { name: 'light', text: 'Light', icon: 'cilSun' },
    { name: 'dark', text: 'Dark', icon: 'cilMoon' },
    { name: 'auto', text: 'Auto', icon: 'cilContrast' },
  ];

  readonly icons = computed(() => {
    const currentMode = this.colorMode();
    return (
      this.colorModes.find((mode) => mode.name === currentMode)?.icon ??
      'cilSun'
    );
  });

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.notifications.refresh();
  }

  sidebarId = input('sidebar1');

  readonly unreadCount = this.notifications.unreadCount;
  readonly recentNotifications = this.notifications.recent;
  readonly notificationLoading = this.notifications.loading;

  onBellOpen(): void {
    if (this.recentNotifications().length === 0) {
      this.notifications.loadRecent();
    }
  }

  onNotificationClick(id: string, relatedAppId: string | undefined): void {
    this.notifications.markAsRead(id);
    if (relatedAppId) {
      this.RouterLink.navigate(['/workflow/application-approval']);
    }
  }

  onMarkAllAsRead(event: MouseEvent): void {
    event.stopPropagation();
    this.notifications.markAllAsRead();
  }

  badgeText(count: number): string {
    return count > 99 ? '99+' : String(count);
  }
}
