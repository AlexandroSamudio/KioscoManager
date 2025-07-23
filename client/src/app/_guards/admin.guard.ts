import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { RoleService } from '../_services/role.service';
import { NotificationService } from '../_services/notification.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const roleService = inject(RoleService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  if (!accountService.isLoggedIn()) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url },
    });
  }

  if (roleService.isAdmin()) {
    return true;
  }

  notificationService.showError('Acceso denegado: Se requieren permisos de administrador');
  return router.createUrlTree(['/dashboard'], {
    queryParams: {
      error: 'Se requieren permisos de administrador para acceder a esta página'
    }
  });
};
