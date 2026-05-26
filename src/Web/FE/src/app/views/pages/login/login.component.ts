import { ChangeDetectorRef, Component } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import { FormsModule } from '@angular/forms';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardGroupComponent,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  FormDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  RowComponent,
} from '@coreui/angular';
import { AuthService } from '../../../../api-authorization/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardGroupComponent,
    CardComponent,
    CardBodyComponent,
    FormDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    IconDirective,
    FormControlDirective,
    ButtonDirective,
    FormsModule,
  ],
})
export class LoginComponent {
  email = '';
  password = '';
  invalid = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
  ) {}

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
