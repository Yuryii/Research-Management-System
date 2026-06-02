import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { StepFlowModalComponent } from '../step-flow-modal/step-flow-modal.component';
import { DocumentCountBadgeComponent } from '../../../shared/components/document-count-badge/document-count-badge.component';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import {
  ApplicationDto,
  ApplicationsClient,
  StepsClient,
  UpdateApplicationCommand,
} from '../../../web-api-client';
import { HttpClient } from '@angular/common/http';
export enum ApplicationStatus {
  Draft = 0,
  Submitted = 1,
}
export interface ApplicationFormData {
  id?: string;
  title: string;
  description: string;
  status: ApplicationStatus;
  files: File[];
  existingFileIds?: string[];
}
@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableDirective,
    ButtonDirective,
    FormSelectDirective,
    FormControlDirective,
    FileUploadModule,
    ToastModule,
    DynamicDialogModule,
    PaginatorModule,
    DocumentCountBadgeComponent,
  ],
  providers: [MessageService, DialogService],
  templateUrl: './applications.component.html',
  styleUrls: ['./applications.component.scss'],
})
export class ApplicationsComponent implements OnInit {
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly applicationService = inject(ApplicationsClient);
  private readonly stepsClient = inject(StepsClient);
  private readonly http = inject(HttpClient);
  applications: ApplicationDto[] = [];
  isLoading = false;
  selectedStatus: ApplicationStatus | null | undefined = null;
  searchTerm = '';
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  currentStepId: string | undefined = undefined;
  readonly rowsPerPageOptions = [5, 10, 20, 50];
  private ref: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadDefaultStepId();
  }

  private loadDefaultStepId(): void {
    this.stepsClient.getDefaultStepIdForUser().subscribe({
      next: (stepId) => {
        if (stepId && stepId !== '00000000-0000-0000-0000-000000000000') {
          this.currentStepId = stepId;
        }
        this.loadApplications();
      },
      error: () => {
        this.currentStepId = undefined;
        this.loadApplications();
      },
    });
  }

  loadApplications(): void {
    this.isLoading = true;
    this.applicationService
      .getApplications(this.pageNumber, this.pageSize, this.currentStepId, undefined, this.selectedStatus ?? undefined, this.searchTerm || undefined)
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

  onStatusFilterChange(value: ApplicationStatus | null | undefined): void {
    this.selectedStatus = value == null || isNaN(value as number) ? undefined : value;
    this.pageNumber = 1;
    this.loadApplications();
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadApplications();
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

  openStepFlowModal(event: Event, application: ApplicationDto): void {
    event.stopPropagation();
    this.ref = this.dialogService.open(StepFlowModalComponent, {
      header: 'Quy trình xử lý',
      width: '70%',
      closable: true,
      draggable: false,
      dismissableMask: true,
      data: { stepDetailId: application.stepDetailId },
    });
  }

  openUpdateModal(application: ApplicationDto): void {
    this.ref = this.dialogService.open(ApplicationModalComponent, {
      header: 'Cập nhật hồ sơ đăng ký',
      width: '60%',
      closable: true,
      draggable: false,
      dismissableMask: true,
      data: { application },
    });

    this.ref?.onClose.subscribe((data: ApplicationFormData) => {
      if (data) {
        const command = new UpdateApplicationCommand({
          id: data.id,
          title: data.title,
          description: data.description,
          status: data.status,
          fileIds: data.existingFileIds,
        });

        this.applicationService
          .updateApplication(application.id, command)
          .subscribe({
            next: () => {
              // Upload files separately after text update succeeds
              if (data.files && data.files.length > 0) {
                const formData = new FormData();
                formData.append('applicationId', application.id);
                formData.append('stepId', application.stepId);
                data.files.forEach((file) => {
                  formData.append('files', file, file.name);
                });

                this.http
                  .post(
                    '/api/ApplicationFiles/CreateApplicationFiles',
                    formData,
                  )
                  .subscribe({
                    next: () => {
                      this.messageService.add({
                        severity: 'success',
                        summary: 'Thành công',
                        detail: 'Hồ sơ đăng ký đã được cập nhật thành công.',
                      });
                      this.loadApplications();
                    },
                    error: (err) => {
                      this.messageService.add({
                        severity: 'warn',
                        summary: 'Cảnh báo',
                        detail:
                          'Cập nhật thông tin thành công nhưng upload file thất bại.',
                      });
                      this.loadApplications();
                    },
                  });
              } else {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Thành công',
                  detail: 'Hồ sơ đăng ký đã được cập nhật thành công.',
                });
                this.loadApplications();
              }
            },
            error: () => {
              this.messageService.add({
                severity: 'error',
                summary: 'Lỗi',
                detail: 'Đã có lỗi xảy ra khi cập nhật hồ sơ đăng ký.',
              });
            },
          });
      }
    });
  }
}
