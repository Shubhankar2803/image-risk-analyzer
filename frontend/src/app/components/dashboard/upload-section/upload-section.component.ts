import { Component, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageAnalysisService } from '../../../services/image-analysis.service';

@Component({
  selector: 'app-upload-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-section.component.html',
  styleUrl: './upload-section.component.css'
})
export class UploadSectionComponent {
  @Output() uploadComplete = new EventEmitter<void>();

  selectedFile: File | null = null;
  dragOver: boolean = false;
  uploading: boolean = false;
  uploadError: string = '';
  uploadSuccess: string = '';

  constructor(
    private imageAnalysisService: ImageAnalysisService,
    private cdr: ChangeDetectorRef
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadError = '';
      this.uploadSuccess = '';
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.selectedFile = event.dataTransfer.files[0];
      this.uploadError = '';
      this.uploadSuccess = '';
    }
  }

  clearFile(): void {
    this.selectedFile = null;
    this.uploadError = '';
    this.uploadSuccess = '';
  }

  uploadImage(): void {
    if (!this.selectedFile) {
      this.uploadError = 'Please select a file first';
      return;
    }

    const maxSizeBytes = 10 * 1024 * 1024;
    if (this.selectedFile.size > maxSizeBytes) {
      this.uploadError = 'File size exceeds 10MB limit';
      return;
    }

    this.uploading = true;
    this.uploadError = '';
    this.uploadSuccess = '';

    this.imageAnalysisService.uploadImage(this.selectedFile).subscribe({
      next: (response: any) => {
        this.uploadSuccess = 'Image analyzed successfully!';
        this.selectedFile = null;
        this.uploading = false;
        this.uploadComplete.emit();
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.uploadError = err.error?.message || 'Upload failed. Please try again.';
        this.uploading = false;
        this.cdr.markForCheck();
      }
    });
  }
}
