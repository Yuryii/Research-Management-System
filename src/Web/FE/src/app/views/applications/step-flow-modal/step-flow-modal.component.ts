import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { StepperModule } from 'primeng/stepper';
import { ButtonDirective } from '@coreui/angular';
import { MessageService } from 'primeng/api';
import { take } from 'rxjs/operators';
import { AuthService } from '../../../../api-authorization/auth.service';
import { Roles } from '../../../../api-authorization/Roles';
import {
  ApplicationDto,
  StepDto,
  StepDetailDto,
  StepsClient,
  ApplicationsClient,
  UpdateApplicationStepDetailCommand,
} from '../../../web-api-client';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { ApiErrorService } from '../../../shared/services/api-error.service';

@Component({
  selector: 'app-step-flow-modal',
  standalone: true,
  imports: [CommonModule, StepperModule, ButtonDirective, TruncatePipe],
  templateUrl: './step-flow-modal.component.html',
  styleUrls: ['./step-flow-modal.component.scss'],
})
export class StepFlowModalComponent implements OnInit {
  ref = inject(DynamicDialogRef);
  config = inject(DynamicDialogConfig);
  private authService = inject(AuthService);
  private stepsClient = inject(StepsClient);
  private applicationsClient = inject(ApplicationsClient);
  private messageService = inject(MessageService);
  private apiErrorService = inject(ApiErrorService);

  allSteps: StepDto[] = [];
  currentStepDetailId: string = '';
  currentStepIndex = 0;
  isLoading = true;
  applicationId: string = '';
  selectedStepDetailId: string = '';
  isUpdating = false;
  userRoles: string[] = [];

  get activeStepDetails(): StepDetailDto[] {
    return this.allSteps[this.currentStepIndex]?.stepDetails ?? [];
  }

  ngOnInit(): void {
    this.currentStepDetailId = this.config.data?.stepDetailId ?? '';
    this.applicationId = this.config.data?.applicationId ?? '';
    this.selectedStepDetailId = this.currentStepDetailId;
    this.authService.roles$.pipe(take(1)).subscribe((roles: string[] | null) => {
      this.userRoles = roles ?? [];
    });
    this.stepsClient.getStepAndStepDetail().subscribe({
      next: (steps: StepDto[]) => {
        this.allSteps = steps;
        this.setCurrentStep();
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

  setCurrentStep(): void {
    const idx = this.allSteps.findIndex((s) =>
      s.stepDetails?.some((d: StepDetailDto) => d.id === this.currentStepDetailId)
    );
    this.currentStepIndex = idx >= 0 ? idx : 0;
  }

  onSelectStepDetail(detailId: string): void {
    this.selectedStepDetailId = detailId;
  }

  onUpdateStepDetail(): void {
    if (!this.applicationId || !this.selectedStepDetailId || this.isUpdating) return;
    this.isUpdating = true;
    const command = new UpdateApplicationStepDetailCommand({
      applicationId: this.applicationId,
      stepDetailId: this.selectedStepDetailId,
    });
    this.applicationsClient.updateApplicationStepDetail(command).subscribe({
      next: () => {
        this.isUpdating = false;
        void this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Bước xử lý đã được cập nhật.',
        });
        this.ref.close({ updated: true });
      },
      error: (err) => {
        this.isUpdating = false;
        this.apiErrorService.showError(
          this.apiErrorService.extractMessage(err),
          'Lỗi',
        );
      },
    });
  }

  close(): void {
    this.ref.close();
  }

  isTeacher(): boolean {
    return this.userRoles.includes(Roles.Teacher);
  }
}
