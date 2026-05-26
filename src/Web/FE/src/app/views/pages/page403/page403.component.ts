import { Component } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonDirective,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-page403',
  templateUrl: './page403.component.html',
  imports: [
    ContainerComponent,
    RowComponent,
    ColComponent,
    InputGroupComponent,
    InputGroupTextDirective,
    IconDirective,
    FormControlDirective,
    ButtonDirective,
  ],
})
export class Page403Component {}
