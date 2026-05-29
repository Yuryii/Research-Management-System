import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { StepperModule } from 'primeng/stepper';
import { ButtonDirective } from '@coreui/angular';
import { StepDto, StepDetailDto, StepsClient } from '../../../web-api-client';
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

  allSteps: StepDto[] = [];
  currentStepDetailId: string = '';
  currentStepIndex = 0;
  isLoading = true;

  get activeStepDetails(): StepDetailDto[] {
    return this.allSteps[this.currentStepIndex]?.stepDetails ?? [];
  }

  ngOnInit(): void {
    this.currentStepDetailId = this.config.data?.stepDetailId ?? '';
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

  close(): void {
    this.ref.close();
  }
}
