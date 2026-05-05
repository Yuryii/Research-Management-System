/**
 * @component NavMenuComponent
 * @description
 * Thành phần điều hướng chính được tùy chỉnh theo giao diện website Trường Đại học Công nghệ Đồng Nai (DNTU).
 *
 * Các thay đổi đã thực hiện:
 * 1. Cấu trúc Header: Thiết kế một lớp (Single layer) hiện đại, chiều cao 100px để làm nổi bật logo chính thức (90px).
 * 2. Hệ màu: Sử dụng mã màu đỏ đặc trưng của DNTU (#a11f21) làm điểm nhấn cho Logo và các mục menu quan trọng.
 * 3. Logics Hiển thị:
 *    - Các liên kết bảo mật (COUNTER, WEATHER, TASKS, NGHIÊN CỨU KHOA HỌC) chỉ hiển thị sau khi người dùng ĐĂNG NHẬP thành công.
 *    - Liên kết "NGHIÊN CỨU KHOA HỌC" trỏ đến trang Quản lý tác vụ (/todo) - trung tâm xử lý của hệ thống.
 *    - Chức năng Đăng nhập/Đăng xuất và các tiện ích (Theme toggle) được bố trí gọn gàng bên phải Header.
 */
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/api-authorization/auth.service';

@Component({
  standalone: false,
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {
  isAuthenticated$ = this.authService.isAuthenticated$;

  constructor(private authService: AuthService, private router: Router) { }

  logout(event: Event): void {
    event.preventDefault();
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login'])
    });
  }
}
