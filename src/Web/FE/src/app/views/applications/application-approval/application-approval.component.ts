import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import {
  BadgeComponent,
  ButtonDirective,
  FormSelectDirective,
  TableDirective,
} from '@coreui/angular';
import {
  DynamicDialogModule,
  DynamicDialogRef,
  DialogService,
} from 'primeng/dynamicdialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import {
  ApplicationDto,
  ApplicationsClient,
  StepsClient,
  StepDetailDto,
  ForwardNextToStepCommand,
  UpdateApplicationStepDetailCommand,
} from '../../../web-api-client';
import { AuthService } from '../../../../api-authorization/auth.service';
import { ReturnApplicationModalComponent } from './return-application-modal/return-application-modal.component';

@Component({
  selector: 'app-application-approval',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableDirective,
    ButtonDirective,
    BadgeComponent,
    FormSelectDirective,
    ToastModule,
    DynamicDialogModule,
    PaginatorModule,
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

  applications: ApplicationDto[] = [];
  isLoading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  readonly rowsPerPageOptions = [5, 10, 20, 50];

  allStepDetails: StepDetailDto[] = [];
  private ref: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadStepDetails();
    this.loadApplications();
  }

  private loadStepDetails(): void {
    this.stepsClient.getStepAndStepDetail().subscribe({
      next: (steps) => {
        this.allStepDetails = steps.flatMap((s) => s.stepDetails ?? []);
      },
    });
  }

  loadApplications(): void {
    this.isLoading = true;
    this.applicationService
      .getApplications(this.pageNumber, this.pageSize, undefined, undefined, undefined)
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

  onStepDetailChange(applicationId: string, stepDetailId: string): void {
    this.applicationService
      .updateApplicationStepDetail(new UpdateApplicationStepDetailCommand({ applicationId, stepDetailId }))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật bước xử lý thành công.',
          });
          this.loadApplications();
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Cập nhật bước xử lý thất bại.',
          });
        },
      });
  }

  onForward(applicationId: string): void {
    this.applicationService
      .forwardNextToStep(new ForwardNextToStepCommand({ applicationId }))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Hồ sơ đã được chuyển tiếp.',
          });
          this.loadApplications();
        },
        error: (err) => {
          const msg = err?.error ?? 'Chuyển tiếp thất bại.';
          this.messageService.add({
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
}
