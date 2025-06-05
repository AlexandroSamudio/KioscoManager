import { Injectable, signal } from '@angular/core';

export interface ScrollPosition {
  x: number;
  y: number;
}

@Injectable({
  providedIn: 'root'
})
export class UiService {
  readonly scrollPosition = signal<ScrollPosition>({ x: 0, y: 0 });
  readonly isScrolled = signal(false);

  scrollToElement(elementId: string, offset: number = 0): void {
    const element = document.getElementById(elementId);
    if (element) {
      const elementPosition = element.getBoundingClientRect().top + window.pageYOffset;
      const offsetPosition = elementPosition - offset;

      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
      });
    }
  }

  openExternalLink(url: string): void {
    window.open(url, '_blank', 'noopener,noreferrer');
  }
}
