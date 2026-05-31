import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { FileUploadModule } from 'primeng/fileupload';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { FormControlDirective } from '@coreui/angular';
import {
  FormGroup,
  FormControl,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ApplicationsClient } from '../../../../web-api-client';

export interface FileParameter {
  data: File;
  fileName: string;
}

@Component({
  selector: 'app-return-application-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ToastModule,
    FileUploadModule,
    FormControlDirective,
  ],
  templateUrl: './return-application-modal.component.html',
  styleUrls: ['./return-application-modal.component.scss'],
})
export class ReturnApplicationModalComponent implements OnInit {
  public ref = inject(DynamicDialogRef);
  public config = inject(DynamicDialogConfig);
  private readonly applicationsClient = inject(ApplicationsClient);
  private readonly messageService = inject(MessageService);

  uploadedFiles: File[] = [];
  isSubmitting = false;

  form = new FormGroup({
    title: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    description: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  ngOnInit(): void {}

  onRemove(event: any): void {
    this.uploadedFiles = this.uploadedFiles.filter((f) => f !== event.file);
  }

  onSelect(event: any): void {
    if (event?.files) {
      this.uploadedFiles.push(...event.files);
    }
  }

  onClear(): void {
    this.uploadedFiles = [];
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, description } = this.form.getRawValue();
    const applicationId = this.config.data?.applicationId ?? '';

    this.isSubmitting = true;

    const files: FileParameter[] = (this.uploadedFiles ?? []).map((file) => ({
      data: file,
      fileName: file.name,
    }));

    this.applicationsClient
      .returnApplication(applicationId, title, description, files)
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Hồ sơ đã được trả về.',
          });
          this.ref.close(true);
        },
        error: (err) => {
          this.isSubmitting = false;
          const msg = err?.error ?? 'Có lỗi xảy ra khi trả hồ sơ.';
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: msg,
          });
        },
      });
  }

  close(): void {
    this.ref.close(false);
  }
}
