import { Component, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { RoleService } from '../../_services/role.service';

export interface ConfigurationSection {
  id: string;
  title: string;
  icon: string;
  description: string;
  route: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent],
  templateUrl: './configuracion.component.html',
  styleUrls: ['./configuracion.component.css']
})
export class ConfiguracionComponent {
  roleService = inject(RoleService);
  private router = inject(Router);

  activeSection = signal<string>('perfil');

  visibleConfigSections = computed(() => {
    return this.configSections.filter(section => this.canAccessSection(section));
  });

  readonly configSections: ConfigurationSection[] = [
    {
      id: 'perfil',
      title: 'Perfil Personal',
      icon: 'fas fa-user',
      description: 'Gestiona tu información personal y preferencias',
      route: '/configuracion/perfil'
    },
    {
      id: 'negocio',
      title: 'Información del Negocio',
      icon: 'fas fa-store',
      description: 'Configuración básica del kiosco',
      route: '/configuracion/negocio',
      adminOnly: true
    },
    {
      id: 'usuarios',
      title: 'Usuarios y Permisos',
      icon: 'fas fa-users',
      description: 'Gestión de roles y códigos de invitación',
      route: '/configuracion/usuarios',
      adminOnly: true
    },
    {
      id: 'inventario',
      title: 'Configuración de Categorías',
      icon: 'fas fa-boxes',
      description: 'Gestión de categorías de productos',
      route: '/configuracion/categorias',
      adminOnly: true
    },
    {
      id: 'reportes',
      title: 'Configuración de Reportes',
      icon: 'fas fa-chart-bar',
      description: 'Formatos de exportación y backups',
      route: '/configuracion/reportes',
      adminOnly: true
    }
  ];

  selectSection(sectionId: string, route: string): void {
    this.activeSection.set(sectionId);
    this.router.navigate([route]);
  }

  isActivePath(route: string): boolean {
    return this.router.url.startsWith(route);
  }

  canAccessSection(section: ConfigurationSection): boolean {
    if (!section.adminOnly) return true;
    return this.roleService.isAdmin();
  }

  getCurrentSectionTitle(): string {
    const currentSection = this.visibleConfigSections().find(s => s.id === this.activeSection());
    return currentSection?.title || 'Configuración';
  }

  getCurrentSectionDescription(): string {
    const currentSection = this.visibleConfigSections().find(s => s.id === this.activeSection());
    return currentSection?.description || 'Gestiona la configuración de tu aplicación';
  }

  isSpecificRouteLoaded(): boolean {
    return this.router.url !== '/configuracion' && this.router.url.startsWith('/configuracion/');
  }

  trackBySectionId = (_: number, s: ConfigurationSection) => s.id;
}
