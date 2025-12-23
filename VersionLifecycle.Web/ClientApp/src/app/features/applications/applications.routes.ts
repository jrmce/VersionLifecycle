import { Routes } from '@angular/router';

export const APPLICATIONS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/applications-list.container').then(m => m.ApplicationsListContainerComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./detail/applications-detail.container').then(m => m.ApplicationsDetailContainerComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/applications-detail.container').then(m => m.ApplicationsDetailContainerComponent)
  }
];
