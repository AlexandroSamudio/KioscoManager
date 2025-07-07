import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { UserService } from '../../../../_services/user.service';
import { NotificationService } from '../../../../_services/notification.service';
import { UserManagement, UserRoleAssignment, UserRoleResponse } from '../../../../_models/user.model';

interface RoleOption {
  value: string;
  label: string;
  description: string;
}

@Component({
  selector: 'app-role-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './role-selector.component.html',
  styleUrls: ['./role-selector.component.css']
})
export class RoleSelectorComponent implements OnInit {
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  @Input() user: UserManagement | null = null;
  @Output() roleAssigned = new EventEmitter<UserRoleResponse>();
  @Output() closeSelector = new EventEmitter<void>();

  isSubmitting = signal<boolean>(false);
  selectedRole = signal<string>('');
  errorMessage = signal<string | null>(null);

  readonly roleOptions: RoleOption[] = [
    {
      value: 'administrador',
      label: 'Administrador',
      description: 'Acceso completo al sistema, incluyendo gestión de usuarios y configuración'
    },
    {
      value: 'empleado',
      label: 'Empleado',
      description: 'Acceso a funciones básicas como ventas e inventario'
    },
    {
      value: 'miembro',
      label: 'Miembro',
      description: 'Sin acceso a ninguna función administrativa'
    }
  ];

  ngOnInit(): void {
    if (this.user?.role) {
      this.selectedRole.set(this.user.role);
    }
  }

  onSubmit(): void {
    if (!this.user || !this.selectedRole() || this.isSubmitting()) {
      return;
    }

    if (this.selectedRole() === this.user.role) {
      this.errorMessage.set('El usuario ya tiene este rol asignado.');
      return;
    }

    const roleAssignment: UserRoleAssignment = {
      role: this.selectedRole()
    };

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.userService.assignRole(this.user.id, roleAssignment)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: (response: UserRoleResponse) => {
          if (response.success) {
            this.notificationService.success(
              'Rol asignado correctamente',
              `El rol "${this.getRoleLabel(response.role)}" ha sido asignado a ${response.username}`
            );
            this.roleAssigned.emit(response);
          } else {
            this.errorMessage.set(response.message || 'No se pudo asignar el rol. Intente nuevamente.');
          }
        },
        error: (error) => {
          this.errorMessage.set(error.message || 'Error al asignar rol. Por favor, inténtalo de nuevo.');
        }
      });
  }

  getRoleLabel(roleValue: string): string {
    const role = this.roleOptions.find(r => r.value.toLowerCase() === roleValue.toLowerCase());
    return role?.label || roleValue;
  }

  onCancel(): void {
    this.closeSelector.emit();
  }

  getRoleClass(role: string): string {
    switch (role.toLowerCase()) {
      case 'administrador':
        return 'bg-amber-200 text-amber-800 border-amber-300';
      case 'empleado':
        return 'bg-blue-200 text-blue-800 border-blue-300';
      case 'miembro':
        return 'bg-green-200 text-green-800 border-green-300';
      default:
        return 'bg-gray-200 text-gray-700 border-gray-300';
    }
  }
}
