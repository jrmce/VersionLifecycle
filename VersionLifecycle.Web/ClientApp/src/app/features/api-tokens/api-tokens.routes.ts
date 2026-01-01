import { Routes } from '@angular/router';

export const API_TOKENS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/api-tokens-list.container').then(m => m.ApiTokensListContainerComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./create/api-token-create.container').then(m => m.ApiTokenCreateContainerComponent)
  }
];
