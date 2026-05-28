import { Component, EventEmitter, OnInit, Output } from '@angular/core';
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
import { ApplicationDto, FileDto } from '../../../../web-api-client';

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
export class ApplicationModalComponent implements OnInit {
  uploadedFiles: File[] = [];
  myApplicationFiles: FileDto[] = [];
  preAttachmentFiles: FileDto[] = [];
  isEditMode = false;
  isReadOnly = false;
  private existingApplication: ApplicationDto | null = null;

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
    isSubmitted: new FormControl<boolean>(false, { nonNullable: true }),
  });

  ngOnInit(): void {
    this.existingApplication = this.config.data?.application ?? null;
    if (this.existingApplication) {
      this.isEditMode = true;
      this.myApplicationFiles = [...(this.existingApplication.myApplications ?? [])];
      this.preAttachmentFiles = [...(this.existingApplication.preAttachments ?? [])];
      if (this.existingApplication.status !== 0) {
        this.isReadOnly = true;
        this.form.disable();
      }
      this.form.patchValue({
        title: this.existingApplication.title,
        description: this.existingApplication.description,
        isSubmitted: this.existingApplication.status !== 0,
      });
    }
  }

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

  removeMyApplicationFile(index: number): void {
    this.myApplicationFiles.splice(index, 1);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { title, description, isSubmitted } = this.form.getRawValue();
    const status = isSubmitted
      ? ApplicationStatus.Submitted
      : ApplicationStatus.Draft;
    const formData: ApplicationFormData = {
      id: this.existingApplication?.id,
      title,
      description,
      status,
      files: this.uploadedFiles,
      existingFileIds: this.myApplicationFiles.map((f) => f.id),
    };
    this.formSubmit.emit(formData);
    this.ref.close(formData);
  }

  close(): void {
    this.ref.close();
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }
}
