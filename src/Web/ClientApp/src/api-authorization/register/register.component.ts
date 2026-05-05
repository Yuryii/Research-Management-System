import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { firstValueFrom } from 'rxjs';

const MIN_PASSWORD_LENGTH = 6;

@Component({
  standalone: false,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  email = '';
  password = '';
  confirmPassword = '';
  emailTouched = false;
  passwordTouched = false;
  confirmPasswordTouched = false;
  error = '';
  showPassword = false;
  showConfirmPassword = false;

  readonly minPasswordLength = MIN_PASSWORD_LENGTH;

  get emailValid() { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email); }
  get passwordValid() { return this.password.length >= MIN_PASSWORD_LENGTH; }
  get passwordsMatch() { return this.password === this.confirmPassword && this.password.length > 0; }

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  async register() {
    this.error = '';
    this.emailTouched = true;
    this.passwordTouched = true;
    this.confirmPasswordTouched = true;
    if (!this.emailValid || !this.passwordValid || !this.passwordsMatch) return;
    try {
      await firstValueFrom(this.authService.register(this.email, this.password));
      await this.router.navigate(['/login']);
    } catch {
      this.error = 'Registration failed. Please try again.';
      this.cdr.detectChanges();
    }
  }
}
