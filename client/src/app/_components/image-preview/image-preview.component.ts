import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-image-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-preview.component.html',
  styleUrl: './image-preview.component.css'
})
export class ImagePreviewComponent {
  @Input() isVisible = false;
  @Input() imageUrl: string | null = null;
  @Input() productName: string = '';
  @Output() close = new EventEmitter<void>();

  loaded = false;

  onBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.closeModal();
    }
  }

  closeModal(): void {
    this.loaded = false;
    this.close.emit();
  }

  onImageLoad(): void {
    this.loaded = true;
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.closeModal();
    }
  }
}
