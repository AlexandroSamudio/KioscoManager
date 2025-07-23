import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

export const authRedirectGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (!accountService.isLoggedIn()) {
    return router.createUrlTree(['/']);
  }

  const kioscoId = accountService.kioscoId();

  if (kioscoId) {
    return router.createUrlTree(['/dashboard']);
  }

  return router.createUrlTree(['/bienvenida']);
};
