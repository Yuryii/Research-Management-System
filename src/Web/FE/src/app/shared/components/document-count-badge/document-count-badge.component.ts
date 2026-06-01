import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BadgeComponent } from '@coreui/angular';
import { IconComponent } from '@coreui/icons-angular';

@Component({
  selector: 'app-document-count-badge',
  standalone: true,
  imports: [CommonModule, BadgeComponent, IconComponent],
  templateUrl: './document-count-badge.component.html',
  styleUrls: ['./document-count-badge.component.scss'],
})
export class DocumentCountBadgeComponent {
  @Input() count: number = 0;
}
