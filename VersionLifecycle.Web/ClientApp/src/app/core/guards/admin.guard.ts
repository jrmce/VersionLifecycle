import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

export const adminGuard: CanActivateFn = () => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  const user = authStore.user();
  if (user?.role === 'SuperAdmin') {
    return true;
  }

  router.navigate(['/dashboard']);
  return false;
};
