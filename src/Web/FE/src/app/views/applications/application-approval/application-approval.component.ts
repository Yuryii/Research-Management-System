import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import {
  ButtonDirective,
} from '@coreui/angular';
import {
  DynamicDialogModule,
  DynamicDialogRef,
  DialogService,
} from 'primeng/dynamicdialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import {
  ApplicationDto,
  StepDto,
  IStepDetailDto,
  ApplicationsClient,
  StepsClient,
  ForwardNextToStepCommand,
  UpdateApplicationCommand,
} from '../../../web-api-client';
import { AuthService } from '../../../../api-authorization/auth.service';
import { ReturnApplicationModalComponent } from './return-application-modal/return-application-modal.component';
import { DocumentCountBadgeComponent } from '../../../shared/components/document-count-badge/document-count-badge.component';
import { ApplicationModalComponent } from '../applications/application-modal/application-modal.component';
import { StepFlowModalComponent } from '../step-flow-modal/step-flow-modal.component';
import { HttpClient } from '@angular/common/http';

export interface StepDetailWithStepName extends IStepDetailDto {
  stepName: string;
}

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
  selector: 'app-application-approval',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonDirective,
    FileUploadModule,
    ToastModule,
    DynamicDialogModule,
    PaginatorModule,
    DocumentCountBadgeComponent,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
  ],
  providers: [MessageService, DialogService],
  templateUrl: './application-approval.component.html',
  styleUrls: ['./application-approval.component.scss'],
})
export class ApplicationApprovalComponent implements OnInit {
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly applicationService = inject(ApplicationsClient);
  private readonly stepsClient = inject(StepsClient);
  private readonly authService = inject(AuthService);
  private readonly http = inject(HttpClient);
  applications: ApplicationDto[] = [];
  isLoading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  currentStepDetailId: string | undefined = undefined;
  readonly rowsPerPageOptions = [5, 10, 20, 50];

  userStepDetails: StepDetailWithStepName[] = [];
  activeTabIndex = 0;

  private ref: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadUserSteps();
  }

  private loadUserSteps(): void {
    this.stepsClient.getMySteps().subscribe({
      next: (steps) => {
        this.userStepDetails = (steps ?? [])
          .flatMap(step => (step.stepDetails ?? []).map(detail => ({ ...detail, stepName: step.name } as StepDetailWithStepName)));
        if (this.userStepDetails.length > 0) {
          this.currentStepDetailId = this.userStepDetails[0].id;
          this.loadApplications();
        }
      },
      error: () => {
        this.userStepDetails = [];
        this.loadApplications();
      },
    });
  }

  onTabChange(index: string | number | undefined): void {
    const idx = typeof index === 'string' ? parseInt(index, 10) : (index ?? 0);
    if (idx >= 0 && idx < this.userStepDetails.length) {
      this.activeTabIndex = idx;
      this.currentStepDetailId = this.userStepDetails[idx].id;
      this.pageNumber = 1;
      this.loadApplications();
    }
  }

  loadApplications(): void {
    this.isLoading = true;
    this.applicationService
      .getApplications(this.pageNumber, this.pageSize, this.currentStepDetailId, undefined, undefined)
      .subscribe({
        next: (result) => {
          this.applications = result?.items ?? [];
          this.totalCount = result?.totalCount ?? 0;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          void this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Không thể tải danh sách hồ sơ.',
          });
        },
      });
  }

  onPageChange(event: PaginatorState): void {
    const nextPageNumber = (event.page ?? 0) + 1;
    const nextPageSize = event.rows ?? this.pageSize;
    if (nextPageNumber === this.pageNumber && nextPageSize === this.pageSize) return;
    this.pageNumber = nextPageNumber;
    this.pageSize = nextPageSize;
    this.loadApplications();
  }

  onForward(applicationId: string): void {
    this.applicationService
      .forwardNextToStep(new ForwardNextToStepCommand({ applicationId }))
      .subscribe({
        next: () => {
          void this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Hồ sơ đã được chuyển tiếp.',
          });
          this.loadApplications();
        },
        error: (err) => {
          const msg = err?.error ?? 'Chuyển tiếp thất bại.';
          void this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: msg,
          });
        },
      });
  }

  openReturnModal(applicationId: string): void {
    this.ref = this.dialogService.open(ReturnApplicationModalComponent, {
      header: 'Trả hồ sơ',
      width: '50%',
      closable: true,
      draggable: false,
      dismissableMask: true,
      data: { applicationId },
    });

    this.ref?.onClose.subscribe((submitted: boolean) => {
      if (submitted) {
        this.loadApplications();
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
              if (data.files && data.files.length > 0) {
                const formData = new FormData();
                formData.append('applicationId', application.id);
                data.files.forEach((file) => {
                  formData.append('files', file, file.name);
                });

                this.http
                  .post('/api/ApplicationFiles/CreateApplicationFiles', formData)
                  .subscribe({
                    next: () => {
                      void this.messageService.add({
                        severity: 'success',
                        summary: 'Thành công',
                        detail: 'Hồ sơ đăng ký đã được cập nhật thành công.',
                      });
                      this.loadApplications();
                    },
                    error: () => {
                      void this.messageService.add({
                        severity: 'warn',
                        summary: 'Cảnh báo',
                        detail: 'Cập nhật thông tin thành công nhưng upload file thất bại.',
                      });
                      this.loadApplications();
                    },
                  });
              } else {
                void this.messageService.add({
                  severity: 'success',
                  summary: 'Thành công',
                  detail: 'Hồ sơ đăng ký đã được cập nhật thành công.',
                });
                this.loadApplications();
              }
            },
            error: () => {
              void this.messageService.add({
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
