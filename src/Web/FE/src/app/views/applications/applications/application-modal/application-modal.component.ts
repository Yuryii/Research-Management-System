import { Component, EventEmitter, Output } from '@angular/core';
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
import {
  ApplicationFormData,
  ApplicationStatus,
} from '../applications.component';

@Component({
  selector: 'app-application-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ToastModule,
    FileUploadModule,
    FormControlDirective,
  ],
  templateUrl: './application-modal.component.html',
  styleUrls: ['./application-modal.component.scss'],
})
export class ApplicationModalComponent {
  uploadedFiles: File[] = [];
  isDownloadingAll = false;
  constructor(
    public ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
  ) {}

  form = new FormGroup({
    title: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    description: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(500)],
    }),
    isDraft: new FormControl<boolean>(false, { nonNullable: true }),
  });

  @Output() formSubmit = new EventEmitter<ApplicationFormData>();

  onClear(): void {
    this.uploadedFiles = [];
  }

  onRemove(event: any): void {
    this.uploadedFiles = this.uploadedFiles.filter((f) => f !== event.file);
  }

  onSelect(event: any): void {
    if (event?.files) {
      this.uploadedFiles.push(...event.files);
    }
  }

  onUpload(event: any): void {
    if (event?.files) {
      this.uploadedFiles.push(...event.files);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { title, description, isDraft } = this.form.getRawValue();
    const status = isDraft
      ? ApplicationStatus.Draft
      : ApplicationStatus.Submitted;
    const formData: ApplicationFormData = {
      title,
      description,
      status,
      files: this.uploadedFiles,
    };
    this.formSubmit.emit(formData);
    this.ref.close(formData);
  }

  downloadAllFiles(): void {
    if (!this.uploadedFiles.length || this.isDownloadingAll) return;
    this.isDownloadingAll = true;
    setTimeout(() => (this.isDownloadingAll = false), 1000);
  }

  close(): void {
    this.ref.close();
  }
}
