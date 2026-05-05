import { Component, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';
import { firstValueFrom } from 'rxjs';

@Component({
  standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  email = '';
  password = '';
  invalid = false;
  showPassword = false;

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  async login() {
    this.invalid = false;
    try {
      await firstValueFrom(this.authService.login(this.email, this.password));
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
      await this.router.navigateByUrl(returnUrl);
    } catch {
      this.invalid = true;
      this.cdr.detectChanges();
    }
  }
}
