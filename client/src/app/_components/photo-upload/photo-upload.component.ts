import { Component, ElementRef, EventEmitter, Input, Output, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../_services/notification.service';

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-upload.component.html'
})
export class PhotoUploadComponent {
  @Input() imageUrl: string | null = null;
  @Input() lowResImageUrl: string | null = null;
  @Input() alt: string = 'Product Image';
  @Output() fileChange = new EventEmitter<File | null>();

  @ViewChild('fileInput') fileInput!: ElementRef;

  private notificationService = inject(NotificationService);

  loaded = false;

  private readonly allowedMimeTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif'];
  private readonly maxFileSize = 5 * 1024 * 1024;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      if (!this.allowedMimeTypes.includes(file.type.toLowerCase())) {
        this.notificationService.error(
          'Tipo de archivo no válido',
          'Solo se permiten archivos JPG, JPEG, PNG, WebP y GIF.'
        );
        this.resetFileInput();
        return;
      }

      if (file.size > this.maxFileSize) {
        this.notificationService.error(
          'Archivo muy grande',
          'El archivo no puede ser mayor a 5MB. Por favor, seleccione un archivo más pequeño.'
        );
        this.resetFileInput();
        return;
      }

      this.fileChange.emit(file);
    }
  }

  private resetFileInput(): void {
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onImageLoad() {
    this.loaded = true;
  }
}
