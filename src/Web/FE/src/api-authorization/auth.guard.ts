import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router,
} from '@angular/router';
import { Observable, combineLatest } from 'rxjs';
import { filter, take, tap, map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
  ): Observable<boolean> {
    const requiredRoles = route.data?.['roles'] as string[] | undefined;

    return combineLatest([
      this.authService.isAuthenticated$,
      this.authService.roles$,
    ]).pipe(
      filter((value) => {
        const [isAuthenticated, roles] = value;
        return isAuthenticated !== null && (!requiredRoles || roles !== null);
      }),
      take(1),
      tap((value) => {
        const [isAuthenticated, roles] = value;
        if (!isAuthenticated) {
          this.router.navigate(['/login'], {
            queryParams: { returnUrl: state.url },
          });
          return;
        }

        if (requiredRoles && !this.hasRequiredRole(roles ?? [], requiredRoles)) {
          this.router.navigate(['/403']);
        }
      }),
      map((value) => {
        const [isAuthenticated, roles] = value;
        if (!isAuthenticated) {
          return false;
        }

        if (!requiredRoles) {
          return true;
        }

        return this.hasRequiredRole(roles ?? [], requiredRoles);
      }),
    );
  }

  private hasRequiredRole(userRoles: string[], requiredRoles: string[]): boolean {
    return requiredRoles.some((role) => userRoles.includes(role));
  }
}
