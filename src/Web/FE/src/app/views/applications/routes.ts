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
          roles: [Roles.Teacher],
        },
      },
      {
        path: 'application-approval',
        loadComponent: () =>
          import('./application-approval/application-approval.component').then(
            (m) => m.ApplicationApprovalComponent,
          ),
        canActivate: [AuthGuard],
        data: {
          title: 'Duyệt hồ sơ',
          roles: [Roles.Administrator, Roles.Tttv, Roles.Dvqltt, Roles.KhcnHtqt],
        },
      },
    ],
  },
];
