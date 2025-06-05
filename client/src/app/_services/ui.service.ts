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
    if (typeof document === 'undefined' || typeof window === 'undefined') {
      return;
    }
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
    if (!url || typeof url !== 'string') {
      console.warn('URL inválida proporcionada a openExternalLink');
      return;
    }

    try {
      new URL(url);
    } catch {
      console.warn('Formato de URL inválido:', url);
      return;
    }

    window.open(url, '_blank', 'noopener,noreferrer');
  }

}
