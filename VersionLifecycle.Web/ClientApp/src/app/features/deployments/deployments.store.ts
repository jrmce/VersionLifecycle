import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, tap, switchMap, catchError, of } from 'rxjs';
import { DeploymentService } from '../../core/services/deployment.service';
import { VersionService } from '../../core/services/version.service';
import { EnvironmentService } from '../../core/services/environment.service';
import { DeploymentDto, DeploymentEventDto, DeploymentStatus, VersionDto, EnvironmentDto, CreatePendingDeploymentDto } from '../../core/models/models';

interface DeploymentsState {
  deployments: DeploymentDto[];
  recentDeployments: DeploymentDto[];
  selectedDeployment: DeploymentDto | null;
  events: DeploymentEventDto[];
  versions: VersionDto[];
  environments: EnvironmentDto[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  skip: number;
  take: number;
}

const initialState: DeploymentsState = {
  deployments: [],
  recentDeployments: [],
  selectedDeployment: null,
  events: [],
  versions: [],
  environments: [],
  loading: false,
  error: null,
  totalCount: 0,
  skip: 0,
  take: 25,
};

export const DeploymentsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ deployments, recentDeployments, totalCount, skip, take }) => ({
    hasDeployments: computed(() => deployments().length > 0),
    recentCount: computed(() => recentDeployments().length),
    currentPage: computed(() => Math.floor(skip() / take()) + 1),
    totalPages: computed(() => Math.ceil(totalCount() / take())),
    hasNextPage: computed(() => skip() + take() < totalCount()),
    hasPreviousPage: computed(() => skip() > 0),
  })),
  withMethods((store, deploymentService = inject(DeploymentService), versionService = inject(VersionService), environmentService = inject(EnvironmentService)) => ({
    loadDeployments: rxMethod<{ skip?: number; take?: number; status?: DeploymentStatus }>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(({ skip = 0, take = 25, status }) =>
          deploymentService.getDeployments(skip, take, status).pipe(
            tap((response) => {
              patchState(store, {
                deployments: response.items,
                totalCount: response.totalCount,
                skip: response.skip,
                take: response.take,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load deployments',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadRecentDeployments: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((count = 5) =>
          deploymentService.getDeployments(0, count).pipe(
            tap((response) => {
              patchState(store, {
                recentDeployments: response.items,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load recent deployments',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadVersions: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((applicationId) =>
          versionService.getVersions(applicationId).pipe(
            tap((versions) => {
              patchState(store, { versions, loading: false });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load versions',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadEnvironments: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((applicationId) =>
          environmentService.getEnvironments(applicationId).pipe(
            tap((environments) => {
              patchState(store, { environments, loading: false });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load environments',
              });
              return of(null);
            })
          )
        )
      )
    ),

    createPendingDeployment: rxMethod<CreatePendingDeploymentDto>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((dto) =>
          deploymentService.createPendingDeployment(dto).pipe(
            tap((deployment) => {
              patchState(store, { selectedDeployment: deployment, loading: false });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to create deployment',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadDeployment: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          deploymentService.getDeployment(id).pipe(
            tap((deployment) => {
              patchState(store, {
                selectedDeployment: deployment,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load deployment',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadDeploymentEvents: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          deploymentService.getDeploymentEvents(id).pipe(
            tap((events) => {
              patchState(store, {
                events,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load deployment events',
              });
              return of(null);
            })
          )
        )
      )
    ),

    confirmDeployment: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          deploymentService.confirmDeployment(id).pipe(
            switchMap(() => deploymentService.getDeployment(id)),
            tap((deployment) => {
              patchState(store, {
                selectedDeployment: deployment,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to confirm deployment',
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
