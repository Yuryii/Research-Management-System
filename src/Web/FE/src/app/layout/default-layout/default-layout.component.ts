import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgScrollbar } from 'ngx-scrollbar';

import { IconDirective } from '@coreui/icons-angular';
import {
  ContainerComponent,
  ShadowOnScrollDirective,
  SidebarBrandComponent,
  SidebarComponent,
  SidebarFooterComponent,
  SidebarHeaderComponent,
  SidebarNavComponent,
  SidebarToggleDirective,
  SidebarTogglerDirective,
  INavData,
} from '@coreui/angular';

import { DefaultFooterComponent, DefaultHeaderComponent } from './';
import { navItems as baseNavItems } from './_nav';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../api-authorization/auth.service';

function isOverflown(element: HTMLElement) {
  return (
    element.scrollHeight > element.clientHeight ||
    element.scrollWidth > element.clientWidth
  );
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './default-layout.component.html',
  styleUrls: ['./default-layout.component.scss'],
  imports: [
    SidebarComponent,
    SidebarHeaderComponent,
    SidebarBrandComponent,
    SidebarNavComponent,
    SidebarFooterComponent,
    SidebarToggleDirective,
    SidebarTogglerDirective,
    ContainerComponent,
    DefaultFooterComponent,
    DefaultHeaderComponent,
    IconDirective,
    NgScrollbar,
    RouterOutlet,
    RouterLink,
    ShadowOnScrollDirective,
  ],
})
export class DefaultLayoutComponent implements OnInit, OnDestroy {
  public navItems: INavData[] = [...baseNavItems];
  public displayedNavItems: INavData[] = [];
  private sub = new Subscription();

  constructor(private auth: AuthService) {}

  ngOnInit(): void {
    this.sub.add(
      this.auth.roles$.subscribe((roles) => {
        const userRoles = roles ?? [];
        this.displayedNavItems = this.filterNavByRoles(
          this.navItems,
          userRoles as string[],
        );
      }),
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  private hasAccess(item: any, userRoles: string[]): boolean {
    if (!item) return false;
    if (!item.roles || !Array.isArray(item.roles) || item.roles.length === 0)
      return true;
    return item.roles.some((r: string) => userRoles.includes(r));
  }

  private filterNavByRoles(items: INavData[], userRoles: string[]): INavData[] {
    return items
      .map((it: any) => {
        const copy = { ...it };
        if (copy.children) {
          copy.children = this.filterNavByRoles(copy.children, userRoles);
        }
        return copy;
      })
      .filter((it: any) => {
        if (it.title) return true;
        if (it.children && it.children.length > 0) {
          return this.hasAccess(it, userRoles) || it.children.length > 0;
        }
        return this.hasAccess(it, userRoles);
      });
  }
}
