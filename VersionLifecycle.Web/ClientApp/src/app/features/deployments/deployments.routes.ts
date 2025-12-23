import { Routes } from '@angular/router';

export const DEPLOYMENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/deployments-list.container').then(m => m.DeploymentsListContainerComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./timeline/deployments-timeline.container').then(m => m.DeploymentsTimelineContainerComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/deployments-detail.container').then(m => m.DeploymentsDetailContainerComponent)
  }
];
