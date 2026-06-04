import { AuthorizeInterceptor } from './../api-authorization/authorize.interceptor';
import { ErrorHandlerInterceptor } from './../api-authorization/error-handler.interceptor';
import {
  APP_ID,
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  provideRouter,
  withEnabledBlockingInitialNavigation,
  withHashLocation,
  withInMemoryScrolling,
  withRouterConfig,
  withViewTransitions,
} from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';
import { routes } from './app.routes';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { API_BASE_URL } from './web-api-client';
import { AuthService } from '../api-authorization/auth.service';

import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';

import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

const MyPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#f5f4fb',
      100: '#e4e2f7',
      200: '#cfc7f3',
      300: '#9d92e6',
      400: '#8379de',
      500: '#5856d6',
      600: '#3634a3',
      700: '#2d2b8b',
      800: '#25246f',
      900: '#1f1e5a',
      950: '#171846',
    },
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff',
          50: '#f8f9fb',
          100: '#f3f4f7',
          200: '#e7eaee',
          300: '#dbdfe6',
          400: '#cfd4de',
          500: '#aab3c5',
          600: '#6d7d9c',
          700: '#4a566d',
          800: '#323a49',
          900: '#212631',
          950: '#080a0c',
        },
      },
      dark: {
        surface: {
          0: '#080a0c',
          50: '#212631',
          100: '#323a49',
          200: '#4a566d',
          300: '#6d7d9c',
          400: '#aab3c5',
          500: '#cfd4de',
          600: '#dbdfe6',
          700: '#e7eaee',
          800: '#f3f4f7',
          900: '#ffffff',
          950: '#ffffff',
        },
      },
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection(),
    provideRouter(
      routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload',
      }),
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled',
      }),
      withEnabledBlockingInitialNavigation(),
      withViewTransitions(),
      withHashLocation(),
    ),

    IconSetService,

    provideAnimationsAsync(),

    providePrimeNG({
      theme: {
        preset: MyPreset,
        options: {
          darkModeSelector: 'html[data-coreui-theme="dark"]',
        },
      },
    }),

    MessageService,

    { provide: APP_ID, useValue: 'ng-cli-universal' },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthorizeInterceptor,
      multi: true,
    },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorHandlerInterceptor,
      multi: true,
    },

    {
      provide: API_BASE_URL,
      useFactory: getApiBaseUrl,
      deps: [],
    },

    provideAppInitializer(() =>
      firstValueFrom(inject(AuthService).initialize()),
    ),

    provideHttpClient(withInterceptorsFromDi()),
  ],
};

export function getApiBaseUrl(): string {
  const url = document.getElementsByTagName('base')[0].href;
  return url.endsWith('/') ? url.slice(0, -1) : url;
}
