import { Routes } from '@angular/router';
import { AuthGuard } from '../../../api-authorization/auth.guard';
import { Roles } from '../../../api-authorization/Roles';

export const routes: Routes = [
  {
    path: '',
    data: {
      title: 'Tổng quát',
    },
    children: [
      {
        path: '',
        redirectTo: 'applications',
        pathMatch: 'full',
      },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/applications.component').then(
            (m) => m.ApplicationsComponent,
          ),
        canActivate: [AuthGuard],
        data: {
          title: 'Hồ sơ',
          roles: [Roles.Administrator, Roles.Teacher],
        },
      },
    ],
  },
];
