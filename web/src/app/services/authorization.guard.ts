import { CanActivateFn, Router } from '@angular/router';
import { AuthorizationService } from './authorization.service';
import { inject } from '@angular/core';

export const authorizationGuard: CanActivateFn = () => {
  const authorizationService = inject(AuthorizationService);
  const router = inject(Router);

  if (!authorizationService.isAuthenticated()) {
    router.navigate(['/master-password']);
    return false;
  }

  return true;
};
