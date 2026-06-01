import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { StepperModule } from 'primeng/stepper';
import { ButtonDirective } from '@coreui/angular';
import { MessageService } from 'primeng/api';
import {
  ApplicationDto,
  StepDto,
  StepDetailDto,
  StepsClient,
  ApplicationsClient,
  UpdateApplicationStepDetailCommand,
} from '../../../web-api-client';
import { TruncatePipe } from '../../../pipes/truncate.pipe';

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
  private stepsClient = inject(StepsClient);
  private applicationsClient = inject(ApplicationsClient);
  private messageService = inject(MessageService);

  allSteps: StepDto[] = [];
  currentStepDetailId: string = '';
  currentStepIndex = 0;
  isLoading = true;
  applicationId: string = '';
  selectedStepDetailId: string = '';
  isUpdating = false;

  get activeStepDetails(): StepDetailDto[] {
    return this.allSteps[this.currentStepIndex]?.stepDetails ?? [];
  }

  ngOnInit(): void {
    this.currentStepDetailId = this.config.data?.stepDetailId ?? '';
    this.applicationId = this.config.data?.applicationId ?? '';
    this.selectedStepDetailId = this.currentStepDetailId;
    this.stepsClient.getStepAndStepDetail().subscribe({
      next: (steps: StepDto[]) => {
        this.allSteps = steps;
        this.setCurrentStep();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
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
        const msg = err?.error ?? 'Cập nhật thất bại.';
        void this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: msg,
        });
      },
    });
  }

  close(): void {
    this.ref.close();
  }
}
