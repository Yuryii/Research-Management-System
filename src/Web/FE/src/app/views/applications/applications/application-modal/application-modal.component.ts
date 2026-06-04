import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { FileUploadModule } from 'primeng/fileupload';
import { DynamicDialogRef, DynamicDialogConfig, DialogService } from 'primeng/dynamicdialog';
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
import { ApplicationDto, FileDto, ApplicationFilesClient } from '../../../../web-api-client';
import { MessageService } from 'primeng/api';
import { StepFlowModalComponent } from '../../step-flow-modal/step-flow-modal.component';
import { AuthService } from '../../../../../api-authorization/auth.service';
import { Roles } from '../../../../../api-authorization/Roles';
import { take } from 'rxjs/operators';
import { ApiErrorService } from '../../../../shared/services/api-error.service';
import { TruncatePipe } from '../../../../shared/pipes/truncate.pipe';

@Component({
  selector: 'app-application-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ToastModule,
    FileUploadModule,
    FormControlDirective,
    TruncatePipe,
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
  isUploadingFiles = false;
  userRoles: string[] = [];
  private existingApplication: ApplicationDto | null = null;

  constructor(
    public ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
    private applicationFilesClient: ApplicationFilesClient,
    private messageService: MessageService,
    private readonly dialogService: DialogService,
    private readonly authService: AuthService,
    private readonly apiErrorService: ApiErrorService,
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
    this.authService.roles$.pipe(take(1)).subscribe((roles) => {
      this.userRoles = roles ?? [];
    });

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

  isTeacher(): boolean {
    return this.userRoles.includes(Roles.Teacher);
  }

  onRemove(event: any): void {
    this.uploadedFiles = this.uploadedFiles.filter((f) => f !== event.file);
  }

  uploadFiles(): void {
    if (!this.existingApplication?.id || this.uploadedFiles.length === 0) return;
    this.isUploadingFiles = true;
    const files: FileParameter[] = this.uploadedFiles.map((file) => ({
      data: file,
      fileName: file.name,
    }));
    this.applicationFilesClient
      .create(this.existingApplication.id, files)
      .subscribe({
        next: () => {
          this.isUploadingFiles = false;
          this.uploadedFiles = [];
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Tệp đã được cập nhật.',
          });
          this.ref.close({ fileUploadSuccess: true });
        },
        error: (err) => {
          this.isUploadingFiles = false;
          this.apiErrorService.showError(
            this.apiErrorService.extractMessage(err),
            'Lỗi',
          );
        },
      });
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

  openStepFlow(): void {
    const stepDetailId = this.existingApplication?.stepDetailId ?? '';
    this.dialogService.open(StepFlowModalComponent, {
      header: 'Chi tiết trang thái',
      width: '70%',
      closable: true,
      draggable: false,
      dismissableMask: true,
      data: { stepDetailId },
    });
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

  downloadFile(file: FileDto): void {
    if (!this.existingApplication?.id) return;
    this.applicationFilesClient
      .downloadApplicationFile(this.existingApplication.id, file.id)
      .subscribe({
        next: (result) => {
          const url = URL.createObjectURL(result.data);
          const a = document.createElement('a');
          a.href = url;
          a.download = file.name;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          URL.revokeObjectURL(url);
        },
        error: (err) => {
          this.apiErrorService.showError(
            this.apiErrorService.extractMessage(err),
            'Lỗi',
          );
        },
      });
  }
}
