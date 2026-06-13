import { INavData } from '@coreui/angular';
import { Roles } from '../../../api-authorization/Roles';

export type NavItem = INavData & { roles?: string[]; children?: NavItem[] };

export const navItems: NavItem[] = [
  {
    name: 'Quản lý hồ sơ & quy trình',
    url: '/workflow',
    iconComponent: { name: 'cil-calculator' },
    roles: [Roles.Administrator, Roles.Teacher, Roles.Tttv, Roles.Dvqltt, Roles.KhcnHtqt],
    children: [
      {
        name: 'Hồ sơ',
        url: '/workflow/applications',
        icon: 'nav-icon-bullet',
        roles: [Roles.Teacher],
      },
      {
        name: 'Duyệt hồ sơ',
        url: '/workflow/application-approval',
        icon: 'nav-icon-bullet',
        roles: [Roles.Administrator, Roles.Tttv, Roles.Dvqltt, Roles.KhcnHtqt],
      },
    ],
  },
];
