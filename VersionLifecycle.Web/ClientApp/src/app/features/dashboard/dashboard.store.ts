import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, tap, forkJoin, switchMap, catchError, of } from 'rxjs';
import { ApplicationService } from '../../core/services/application.service';
import { DeploymentService } from '../../core/services/deployment.service';
import { ApplicationDto, DeploymentDto } from '../../core/models/models';

interface DashboardState {
  applications: ApplicationDto[];
  recentDeployments: DeploymentDto[];
  loading: boolean;
  error: string | null;
}

const initialState: DashboardState = {
  applications: [],
  recentDeployments: [],
  loading: false,
  error: null,
};

export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ applications, recentDeployments, loading }) => ({
    hasData: computed(() => applications().length > 0 || recentDeployments().length > 0),
    isEmpty: computed(() => !loading() && applications().length === 0 && recentDeployments().length === 0),
  })),
  withMethods((
    store,
    applicationService = inject(ApplicationService),
    deploymentService = inject(DeploymentService)
  ) => ({
    loadDashboard: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(() =>
          forkJoin({
            apps: applicationService.getApplications(0, 5),
            deps: deploymentService.getDeployments(0, 5),
          }).pipe(
            tap(({ apps, deps }) => {
              patchState(store, {
                applications: apps.items,
                recentDeployments: deps.items,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load dashboard data',
              });
              return of(null);
            })
          )
        )
      )
    ),

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
