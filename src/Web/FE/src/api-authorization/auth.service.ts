import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map, switchMap } from 'rxjs/operators';
import {
  LoginRequest,
  RegisterRequest,
  UsersClient,
} from '../app/web-api-client';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _isAuthenticated = new BehaviorSubject<boolean | null>(null);
  private _roles = new BehaviorSubject<string[] | null>(null);
  isAuthenticated$ = this._isAuthenticated.asObservable();
  roles$ = this._roles.asObservable();

  constructor(private usersClient: UsersClient) {}

  initialize(): Observable<boolean> {
    return this.usersClient.getInfoWithRoles().pipe(
      map((info): boolean => {
        this._roles.next(this.normalizeRoles(info));
        return true;
      }),
      catchError((): Observable<boolean> => {
        this._roles.next([]);
        return of(false);
      }),
      tap((isAuth: boolean) => {
        this._isAuthenticated.next(isAuth);
        console.log(`Auth initialized. Authenticated: ${isAuth}`);
      }),
    );
  }

  login(email: string, password: string): Observable<void> {
    return this.usersClient
      .login(true, undefined, new LoginRequest({ email, password }))
      .pipe(
        switchMap(() => this.initialize()),
        map(() => void 0),
      );
  }

  register(email: string, password: string): Observable<void> {
    return this.usersClient.register(new RegisterRequest({ email, password }));
  }

  logout(): Observable<void> {
    return this.usersClient.logout({}).pipe(
      tap(() => {
        this._isAuthenticated.next(false);
        this._roles.next([]);
      }),
    );
  }

  private normalizeRoles(info: unknown): string[] {
    if (!info || typeof info !== 'object') {
      return [];
    }

    const record = info as Record<string, unknown>;
    const rolesValue = record['roles'] ?? record['roleNames'] ?? record['role'];

    if (Array.isArray(rolesValue)) {
      return rolesValue.filter((role) => typeof role === 'string') as string[];
    }

    if (typeof rolesValue === 'string') {
      return [rolesValue];
    }

    return [];
  }
}
