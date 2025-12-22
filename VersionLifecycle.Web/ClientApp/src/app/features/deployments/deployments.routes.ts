import { Routes } from '@angular/router';

export const DEPLOYMENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/deployments-list.component').then(m => m.DeploymentsListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./timeline/deployments-timeline.component').then(m => m.DeploymentsTimelineComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/deployments-detail.component').then(m => m.DeploymentsDetailComponent)
  }
];
