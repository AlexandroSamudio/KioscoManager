import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';

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
  activeSection = signal<string>('perfil');

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
      description: 'Gestión de empleados y roles',
      route: '/configuracion/usuarios',
      adminOnly: true
    },
    {
      id: 'inventario',
      title: 'Configuración de Inventario',
      icon: 'fas fa-boxes',
      description: 'Gestion de categorías de productos',
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

  constructor(private router: Router) {}

  selectSection(sectionId: string, route: string): void {
    this.activeSection.set(sectionId);
    this.router.navigate([route]);
  }

  isActivePath(route: string): boolean {
    return this.router.url.startsWith(route);
  }

  // TODO: Implementar verificación de roles de usuario
  isAdmin(): boolean {
    return true;
  }

  canAccessSection(section: ConfigurationSection): boolean {
    if (!section.adminOnly) return true;
    return this.isAdmin();
  }

  getCurrentSectionTitle(): string {
    const currentSection = this.configSections.find(s => s.id === this.activeSection());
    return currentSection?.title || 'Configuración';
  }

  getCurrentSectionDescription(): string {
    const currentSection = this.configSections.find(s => s.id === this.activeSection());
    return currentSection?.description || 'Gestiona la configuración de tu aplicación';
  }

  isSpecificRouteLoaded(): boolean {
    return this.router.url !== '/configuracion' && this.router.url.startsWith('/configuracion/');
  }

  hasUnsavedChanges(): boolean {
    // TODO: Implementar lógica para detectar cambios no guardados
    return false;
  }
}
