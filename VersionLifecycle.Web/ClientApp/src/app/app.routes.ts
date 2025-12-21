import { Routes } from '@angular/router';
import { AuthGuardService } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    canActivate: [AuthGuardService],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'applications',
    canActivate: [AuthGuardService],
    loadChildren: () => import('./features/applications/applications.routes').then(m => m.APPLICATIONS_ROUTES)
  },
  {
    path: 'deployments',
    canActivate: [AuthGuardService],
    loadChildren: () => import('./features/deployments/deployments.routes').then(m => m.DEPLOYMENTS_ROUTES)
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
