import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import {
  BadgeComponent,
  ButtonDirective,
  FormControlDirective,
  FormSelectDirective,
  TableDirective,
} from '@coreui/angular';
import {
  DynamicDialogModule,
  DynamicDialogRef,
  DialogService,
} from 'primeng/dynamicdialog';
import { IconComponent } from '@coreui/icons-angular';
import { ApplicationModalComponent } from './application-modal/application-modal.component';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ApplicationDto, ApplicationsClient } from '../../../web-api-client';
export enum ApplicationStatus {
  Draft = 0,
  Submitted = 1,
}
export interface ApplicationFormData {
  title: string;
  description: string;
  status: ApplicationStatus;
  files: File[];
}
@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [
    CommonModule,
    TableDirective,
    ButtonDirective,
    IconComponent,
    BadgeComponent,
    ButtonDirective,
    FormSelectDirective,
    FormControlDirective,
    FileUploadModule,
    ToastModule,
    DynamicDialogModule,
    PaginatorModule,
  ],
  providers: [MessageService, DialogService],
  templateUrl: './applications.component.html',
  styleUrls: ['./applications.component.scss'],
})
export class ApplicationsComponent implements OnInit {
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly applicationService = inject(ApplicationsClient);
  applications: ApplicationDto[] = [];
  isLoading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  readonly rowsPerPageOptions = [5, 10, 20, 50];
  private ref: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading = true;
    this.applicationService
      .getApplications(this.pageNumber, this.pageSize, undefined)
      .subscribe({
        next: (result) => {
          this.applications = result?.items ?? [];
          this.totalCount = result?.totalCount ?? 0;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Không thể tải danh sách hồ sơ đăng ký.',
          });
        },
      });
  }

  onPageChange(event: PaginatorState): void {
    const nextPageNumber = (event.page ?? 0) + 1;
    const nextPageSize = event.rows ?? this.pageSize;

    if (nextPageNumber === this.pageNumber && nextPageSize === this.pageSize) {
      return;
    }

    this.pageNumber = nextPageNumber;
    this.pageSize = nextPageSize;
    this.loadApplications();
  }

  openModal(): void {
    this.ref = this.dialogService.open(ApplicationModalComponent, {
      header: 'Tạo hồ sơ đăng ký',
      width: '60%',
      closable: true,
      draggable: false,
      dismissableMask: true,
    });

    this.ref?.onClose.subscribe((data: ApplicationFormData) => {
      if (data) {
        const files: FileParameter[] = (data.files ?? []).map((file) => ({
          data: file,
          fileName: file.name,
        }));

        this.applicationService
          .createApplication(data.title, data.description, data.status, files)
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Thành công',
                detail: 'Hồ sơ đăng ký đã được tạo thành công.',
              });
              this.loadApplications();
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Lỗi',
                detail: 'Đã có lỗi xảy ra khi tạo hồ sơ đăng ký.',
              });
            },
          });
      }
    });
  }
}
