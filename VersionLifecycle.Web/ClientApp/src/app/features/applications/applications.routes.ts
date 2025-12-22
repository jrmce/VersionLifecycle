import { Routes } from '@angular/router';

export const APPLICATIONS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/applications-list.component').then(m => m.ApplicationsListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./detail/applications-detail.component').then(m => m.ApplicationsDetailComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/applications-detail.component').then(m => m.ApplicationsDetailComponent)
  }
];
