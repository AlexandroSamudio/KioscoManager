import { computed, inject, Injectable } from '@angular/core';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private accountService = inject(AccountService);

  isAdmin = computed(() =>
    this.hasRole(['administrador', 'admin'])
  );

  isEmpleado = computed(() =>
    this.hasRole(['empleado', 'administrador', 'admin'])
  );

  isMiembro = computed(() =>
    this.hasRole(['miembro'])
  );

  get userRoles() {
    return this.accountService.roles() || [];
  }

  hasRole(allowedRoles: string[]): boolean {
    if (!allowedRoles?.length) return false;

    const userRoles = this.userRoles;
    if (!userRoles?.length) return false;

    return allowedRoles.some(allowedRole =>
      userRoles.some(userRole =>
        userRole.toLowerCase() === allowedRole.toLowerCase()
      )
    );
  }

  canAccess(feature: 'reportes' | 'usuarios' | 'categorias' | 'negocio'): boolean {
    switch (feature) {
      case 'reportes':
      case 'usuarios':
      case 'categorias':
      case 'negocio':
        return this.isAdmin();
      default:
        return false;
    }
  }

  canPerformActions(): boolean {
    return !this.isMiembro();
  }
}
