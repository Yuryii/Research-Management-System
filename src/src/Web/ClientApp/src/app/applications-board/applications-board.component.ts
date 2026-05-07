import { Component, computed, signal } from '@angular/core';

const Roles = {
  Administrator: 'Administrator',
  Teacher: 'Teacher',
  Tttv: 'Tttv',
  Dvqltt: 'Dvqltt',
  KhcnHtqt: 'KhcnHtqt'
} as const;

type RoleKey = keyof typeof Roles;

type ApplicationRow = {
  id: number;
  title: string;
  description: string;
  attachments: number;
  teacherName: string;
  myAttachments: number;
  status: string;
  teacherStatus: 'Draft' | 'Submitted';
};

@Component({
  standalone: false,
  selector: 'app-applications-board',
  templateUrl: './applications-board.component.html'
})
export class ApplicationsBoardComponent {
  roles = Object.values(Roles);
  currentRole = signal<string>(Roles.Teacher);

  tabs = ['Preliminary check', 'Deposit verification'];
  activeTab = signal(this.tabs[0]);

  applications = signal<ApplicationRow[]>([
    {
      id: 1,
      title: 'Item 1',
      description: 'Item 1',
      attachments: 16,
      teacherName: 'Item 1',
      myAttachments: 12,
      status: 'Forwarded to KHCN - HTQT',
      teacherStatus: 'Submitted'
    },
    {
      id: 2,
      title: 'Item 2',
      description: 'Item 2',
      attachments: 16,
      teacherName: 'Item 2',
      myAttachments: 0,
      status: 'Preliminary review',
      teacherStatus: 'Draft'
    },
    {
      id: 3,
      title: 'Item 3',
      description: 'Item 3',
      attachments: 16,
      teacherName: 'Item 3',
      myAttachments: 3,
      status: 'Confirmed on the Request Form',
      teacherStatus: 'Submitted'
    },
    {
      id: 4,
      title: 'Item 1',
      description: 'Item 1',
      attachments: 16,
      teacherName: 'Item 1',
      myAttachments: 0,
      status: 'Forwarded ALL to KHCN - HTQT',
      teacherStatus: 'Draft'
    },
    {
      id: 5,
      title: 'Item 2',
      description: 'Item 2',
      attachments: 16,
      teacherName: 'Item 2',
      myAttachments: 0,
      status: 'Forwarded to KHCN - HTQT',
      teacherStatus: 'Submitted'
    }
  ]);

  isTeacher = computed(() => this.currentRole() === Roles.Teacher);
  isNonTeacher = computed(() => this.currentRole() !== Roles.Teacher);

  setRole(role: string): void {
    this.currentRole.set(role);
  }

  setTab(tab: string): void {
    this.activeTab.set(tab);
  }
}
