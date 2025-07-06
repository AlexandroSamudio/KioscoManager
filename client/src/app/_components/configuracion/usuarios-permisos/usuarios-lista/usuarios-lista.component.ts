import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { UserService } from '../../../../_services/user.service';
import { NotificationService } from '../../../../_services/notification.service';
import { AccountService } from '../../../../_services/account.service';
import { UserManagement, UserRoleResponse } from '../../../../_models/user.model';
import { Pagination } from '../../../../_services/pagination.helper';
import { RoleSelectorComponent } from '../role-selector/role-selector.component';

@Component({
  selector: 'app-usuarios-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, RoleSelectorComponent],
  templateUrl: './usuarios-lista.component.html',
  styleUrls: ['./usuarios-lista.component.css']
})
export class UsuariosListaComponent implements OnInit {
  private userService = inject(UserService);
  private destroyRef = inject(DestroyRef);
  private notificationService = inject(NotificationService);
  private accountService = inject(AccountService);

  protected Math = Math;

  users = signal<UserManagement[]>([]);
  pagination = signal<Pagination | null>(null);
  isLoading = signal<boolean>(false);
  selectedUser = signal<UserManagement | null>(null);
  currentKioscoId = signal<number | null>(null);

  currentPage = signal<number>(1);
  pageSize = signal<number>(5);

  pages = computed<number[]>(() => {
    const totalPages = this.pagination()?.totalPages || 1;
    const currentPage = this.pagination()?.currentPage || 1;

    const pages: number[] = [];

    pages.push(1);

    const startPage = Math.max(2, currentPage - 1);
    const endPage = Math.min(totalPages - 1, currentPage + 1);

    if (startPage > 2) {
      pages.push(-1);
    }

    for (let i = startPage; i <= endPage; i++) {
      if (i !== 1 && i !== totalPages) {
        pages.push(i);
      }
    }

    if (endPage < totalPages - 1) {
      pages.push(-1);
    }

    if (totalPages > 1) {
      pages.push(totalPages);
    }

    return pages;
  });

  ngOnInit(): void {
    this.getCurrentKioscoId();
  }

  private getCurrentKioscoId(): void {
    const kioscoId = this.accountService.kioscoId();

    if (!kioscoId) {
      this.notificationService.error(
        'Error',
        'No se pudo determinar el kiosco actual. Por favor, inicie sesión nuevamente.'
      );
    }

    const kioscoIdNumber = typeof kioscoId === 'string' ? parseInt(kioscoId, 10) : kioscoId;

    this.currentKioscoId.set(kioscoIdNumber);
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);

    const kioscoId = this.currentKioscoId();
    if (!kioscoId) {
      this.notificationService.error(
        'Error',
        'No se pudo determinar el kiosco actual'
      );
      this.isLoading.set(false);
      this.users.set([]);
      this.pagination.set(null);
      return;
    }

    this.userService.getUsersByKioscoPaginated(kioscoId, this.currentPage(), this.pageSize())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.users.set(response.result);
          this.pagination.set(response.pagination);
        },
        error: (error) => {
          console.error('Error al cargar usuarios del kiosco paginados', error);
          this.notificationService.error(
            'Error',
            'No se pudieron cargar los usuarios. Por favor, intente nuevamente.'
          );
        }
      });
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadUsers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.loadUsers();
  }

  selectUser(user: UserManagement): void {
    if (this.selectedUser()?.id === user.id) {
      this.selectedUser.set(null);
    } else {
      this.selectedUser.set(user);
    }
  }

  handleRoleAssigned(response: UserRoleResponse): void {
    const updatedUsers = this.users().map(user => {
      if (user.id === response.userId) {
        return { ...user, role: response.role };
      }
      return user;
    });

    this.users.set(updatedUsers);
    this.selectedUser.set(null);
  }

  closeRoleSelector(): void {
    this.selectedUser.set(null);
  }

  getRoleClass(role: string | undefined): string {
    if (!role) return 'bg-gray-200 text-gray-700';

    switch (role.toLowerCase()) {
      case 'administrador':
        return 'bg-amber-200 text-amber-800';
      case 'empleado':
        return 'bg-blue-200 text-blue-800';
      case 'miembro':
        return 'bg-green-200 text-green-800';
      default:
        return 'bg-gray-200 text-gray-700';
    }
  }

  trackByUserId(index: number, user: UserManagement): number {
    return user.id;
  }
}
