import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom, forkJoin } from 'rxjs';
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
  withMethods((store, applicationService = inject(ApplicationService), deploymentService = inject(DeploymentService)) => ({
    async loadDashboard() {
      patchState(store, { loading: true, error: null });
      try {
        const { apps, deps } = await firstValueFrom(
          forkJoin({
            apps: applicationService.getApplications(0, 5),
            deps: deploymentService.getDeployments(0, 5),
          })
        );
        patchState(store, {
          applications: apps.items,
          recentDeployments: deps.items,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load dashboard data',
        });
      }
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
