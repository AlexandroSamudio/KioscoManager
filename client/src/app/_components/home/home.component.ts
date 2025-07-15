import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { UiService } from '../../_services/ui.service';
import {
  HomeDataService,
  Feature,
  Testimonial,
  Benefit,
  FooterLink,
  FooterSection,
} from '../../_services/home-data.service';

interface NavItem {
  label: string;
  section: string;
}
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  private readonly router = inject(Router);
  private readonly uiService = inject(UiService);
  private readonly homeDataService = inject(HomeDataService);

  readonly isMenuOpen = signal(false);
  readonly benefits = this.homeDataService.benefits;
  readonly currentYear = new Date().getFullYear();
  readonly footerSections = this.homeDataService.footerSections;
  readonly testimonials = this.homeDataService.testimonials;
  readonly socialLinks = this.homeDataService.socialLinks;
  dashboardImage: string =
    'https://images.unsplash.com/photo-1472851294608-062f824d29cc?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D';

  navItems: NavItem[] = [
    { label: 'Características', section: 'features' },
    { label: 'Testimonios', section: 'testimonials' },
    { label: 'Contacto', section: 'contact' },
  ];

  toggleMenu(): void {
    this.isMenuOpen.update((open) => !open);
  }

  scrollToSection(sectionId: string): void {
    this.uiService.scrollToElement(sectionId);
    this.isMenuOpen.set(false);
  }
  readonly features = this.homeDataService.features;
  getIconPath(iconName: string): string {
    const iconPaths: Record<string, string> = {
      package:
        'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4',
      'shopping-cart':
        'M3 3h2l.4 2M7 13h10l4-8H5.4m1.6 8L6 5H3m4 8a2 2 0 100 4 2 2 0 000-4zm10 0a2 2 0 100 4 2 2 0 000-4z',
      'alert-triangle':
        'M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0zM12 9v4m0 4h.01',
      'bar-chart-3': 'M3 3v18h18M8 17V9m4 8V5m4 12v-7',
      users:
        'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2m8-10a4 4 0 100-8 4 4 0 000 8zm8 6v-2a4 4 0 00-3-3.87m-3-12a4 4 0 010 7.75',
      shield: 'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z',
      clock: 'M12 6v6l4 2',
      'trending-up': 'M3 17l6-6 4 4 8-8',
      'trending-down': 'M3 7l6 6 4-4 8 8',
    };
    return iconPaths[iconName] ?? '';
  }

  getStarsArray(rating: number): readonly number[] {
    return Array(rating)
      .fill(0)
      .map((_, i) => i);
  }

  trackByTestimonialId(index: number, testimonial: Testimonial): number {
    return testimonial.id;
  }

  trackByFeatureTitle(index: number, feature: Feature): string {
    return feature.title;
  }

  onLinkClick(link: FooterLink): void {
    try {
      if (link.external) {
        this.uiService.openExternalLink(link.url);
      } else if (link.url.startsWith('#')) {
        const sectionId = link.url.substring(1);
        this.uiService.scrollToElement(sectionId);
      } else {
        console.log('Navegando a:', link.url);
      }
    } catch (error) {
      console.error('Error al navegar:', error);
    }
  }

  getSocialIconPath(iconName: string): string {
    const iconPaths: Record<string, string> = {
      facebook:
        'M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z',
      twitter:
        'M23 3a10.9 10.9 0 01-3.14 1.53 4.48 4.48 0 00-7.86 3v1A10.66 10.66 0 013 4s-4 9 5 13a11.64 11.64 0 01-7 2c9 5 20 0 20-11.5a4.5 4.5 0 00-.08-.83A7.72 7.72 0 0023 3z',
      linkedin:
        'M16 8a6 6 0 016 6v7h-4v-7a2 2 0 00-2-2 2 2 0 00-2 2v7h-4v-7a6 6 0 016-6zM2 9h4v12H2zM4 2a2 2 0 100 4 2 2 0 000-4z',
      instagram:
        'M7 2h10a5 5 0 015 5v10a5 5 0 01-5 5H7a5 5 0 01-5-5V7a5 5 0 015-5zm0 2a3 3 0 00-3 3v10a3 3 0 003 3h10a3 3 0 003-3V7a3 3 0 00-3-3H7zm10.5 2.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM12 7a5 5 0 110 10 5 5 0 010-10zm0 2a3 3 0 100 6 3 3 0 000-6z',
    };
    return iconPaths[iconName] ?? '';
  }

  trackBySection(index: number, section: FooterSection): string {
    return section.title;
  }

  trackByLink(index: number, link: FooterLink): string {
    return link.text;
  }
  trackByBenefit(index: number, benefit: Benefit): string {
    return benefit.title;
  }

  redirectTo(path: string) {
    this.router.navigate([path]);
  }
}
