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
  ApplicationsClient,
  StepsClient,
  ApplicationFilesClient,
  ForwardNextToStepCommand,
  UpdateApplicationCommand,
} from '../../../web-api-client';
import { AuthService } from '../../../../api-authorization/auth.service';
import { Roles } from '../../../../api-authorization/Roles';
import { ReturnApplicationModalComponent } from './return-application-modal/return-application-modal.component';
import { DocumentCountBadgeComponent } from '../../../shared/components/document-count-badge/document-count-badge.component';
import { ApplicationModalComponent } from '../applications/application-modal/application-modal.component';
import { StepFlowModalComponent } from '../step-flow-modal/step-flow-modal.component';
import { ApiErrorService } from '../../../shared/services/api-error.service';
import { TruncatePipe } from '../../../shared/pipes/truncate.pipe';

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
    FormControlDirective,
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
    TruncatePipe,
  ],
  providers: [DialogService],
  templateUrl: './application-approval.component.html',
  styleUrls: ['./application-approval.component.scss'],
})
export class ApplicationApprovalComponent implements OnInit {
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly applicationService = inject(ApplicationsClient);
  private readonly stepsClient = inject(StepsClient);
  private readonly authService = inject(AuthService);
  private readonly applicationFilesClient = inject(ApplicationFilesClient);
  private readonly apiErrorService = inject(ApiErrorService);
  applications: ApplicationDto[] = [];
  isLoading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  currentStepId: string | undefined = undefined;
  readonly rowsPerPageOptions = [5, 10, 20, 50];

  userSteps: StepDto[] = [];
  activeTabIndex = 0;
  searchTerm = '';
  roles: string[] | null = null;
  readonly Roles = Roles;

  private ref: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.authService.roles$.subscribe((roles) => {
      this.roles = roles;
    });
    this.loadUserSteps();
  }

  private loadUserSteps(): void {
    this.stepsClient.getMySteps().subscribe({
      next: (steps) => {
        this.userSteps = steps ?? [];
        if (this.userSteps.length > 0) {
          this.currentStepId = this.userSteps[0].id;
          this.loadApplications();
        }
      },
      error: () => {
        this.userSteps = [];
        this.loadApplications();
      },
    });
  }

  onTabChange(index: string | number | undefined): void {
    const idx = typeof index === 'string' ? parseInt(index, 10) : (index ?? 0);
    if (idx >= 0 && idx < this.userSteps.length) {
      this.activeTabIndex = idx;
      this.currentStepId = this.userSteps[idx].id;
      this.pageNumber = 1;
      this.loadApplications();
    }
  }

  loadApplications(): void {
    this.isLoading = true;
    this.applicationService
      .getApplications(this.pageNumber, this.pageSize, undefined, this.currentStepId, undefined, this.searchTerm || undefined)
      .subscribe({
        next: (result) => {
          this.applications = result?.items ?? [];
          this.totalCount = result?.totalCount ?? 0;
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.apiErrorService.showError(
            this.apiErrorService.extractMessage(err),
            'Lỗi',
          );
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

  onSearch(): void {
    this.pageNumber = 1;
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
          this.apiErrorService.showError(this.apiErrorService.extractMessage(err), 'Lỗi');
        },
      });
  }

  openReturnModal(applicationId: string, createdBy?: string): void {
    this.ref = this.dialogService.open(ReturnApplicationModalComponent, {
      header: 'Trả hồ sơ',
      width: '50%',
      closable: true,
      draggable: false,
      dismissableMask: true,
      data: { applicationId, createdBy },
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
      data: { stepDetailId: application.stepDetailId, applicationId: application.id },
    });

    this.ref?.onClose.subscribe((result: any) => {
      if (result?.updated) {
        this.loadApplications();
      }
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

    this.ref?.onClose.subscribe((data: ApplicationFormData | { fileUploadSuccess?: boolean }) => {
      if (!data) return;
      if ('fileUploadSuccess' in data && data.fileUploadSuccess) {
        this.loadApplications();
        return;
      }
      const formData = data as ApplicationFormData;
        const command = new UpdateApplicationCommand({
          id: formData.id,
          title: formData.title,
          description: formData.description,
          status: formData.status,
          fileIds: formData.existingFileIds,
        });

        this.applicationService
          .updateApplication(application.id, command)
          .subscribe({
            next: () => {
              if (formData.files && formData.files.length > 0) {
                const fileParameters = formData.files.map((file) => ({
                  data: file,
                  fileName: file.name,
                }));

                this.applicationFilesClient
                  .create(application.id, fileParameters)
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
            error: (err) => {
              this.apiErrorService.showError(
                this.apiErrorService.extractMessage(err),
                'Lỗi',
              );
            },
          });
    });
  }
}
