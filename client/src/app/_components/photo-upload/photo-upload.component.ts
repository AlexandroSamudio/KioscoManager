import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-upload.component.html',
  styleUrls: ['./photo-upload.component.css']
})
export class PhotoUploadComponent {
  @Input() imageUrl: string | null = null;
  @Input() lowResImageUrl: string | null = null;
  @Input() alt: string = 'Product Image';
  @Output() fileChange = new EventEmitter<File | null>();

  @ViewChild('fileInput') fileInput!: ElementRef;

  loaded = false;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.fileChange.emit(file);
    }
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onImageLoad() {
    this.loaded = true;
  }
}
