export const Roles = {
  Administrator: 'Administrator',
  Teacher: 'Teacher',
  Tttv: 'Tttv',
  Dvqltt: 'Dvqltt',
  KhcnHtqt: 'KhcnHtqt',
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];
