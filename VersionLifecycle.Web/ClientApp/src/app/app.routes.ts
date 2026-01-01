import { Routes } from '@angular/router';
import { AuthGuardService } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/how-to-use',
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
    path: 'how-to-use',
    loadComponent: () => import('./features/how-to-use/how-to-use.component').then(m => m.HowToUseComponent)
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
    path: 'environments',
    canActivate: [AuthGuardService],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./features/environments/list/environments-list.container').then(m => m.EnvironmentsListContainerComponent)
      },
      {
        path: 'new',
        loadComponent: () => import('./features/environments/edit/environment-edit.container').then(m => m.EnvironmentEditContainerComponent)
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [AuthGuardService, adminGuard],
    children: [
      {
        path: 'tenants',
        loadComponent: () => import('./features/admin/tenants/tenants-list.container').then(m => m.TenantsListContainerComponent)
      },
      {
        path: 'tenants/create',
        loadComponent: () => import('./features/admin/tenants/tenant-create.container').then(m => m.TenantCreateContainerComponent)
      },
      {
        path: 'tenants/:id/edit',
        loadComponent: () => import('./features/admin/tenants/tenant-edit.container').then(m => m.TenantEditContainerComponent)
      },
      {
        path: 'tenants/:id/stats',
        loadComponent: () => import('./features/admin/tenants/tenant-stats.component').then(m => m.TenantStatsComponent)
      }
    ]
  },
  {
    path: 'api-tokens',
    canActivate: [AuthGuardService, adminGuard],
    loadChildren: () => import('./features/api-tokens/api-tokens.routes').then(m => m.API_TOKENS_ROUTES)
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
