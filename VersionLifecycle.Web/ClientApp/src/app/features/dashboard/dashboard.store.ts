import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom, forkJoin } from 'rxjs';
import { ApplicationService } from '../../core/services/application.service';
import { DeploymentService } from '../../core/services/deployment.service';
import { EnvironmentService } from '../../core/services/environment.service';
import { ApplicationDto, DeploymentDto, EnvironmentDeploymentOverview } from '../../core/models/models';

interface DashboardState {
  applications: ApplicationDto[];
  recentDeployments: DeploymentDto[];
  environments: EnvironmentDeploymentOverview[];
  loading: boolean;
  error: string | null;
}

const initialState: DashboardState = {
  applications: [],
  recentDeployments: [],
  environments: [],
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
  withMethods((store, applicationService = inject(ApplicationService), deploymentService = inject(DeploymentService), environmentService = inject(EnvironmentService)) => ({
    async loadDashboard() {
      patchState(store, { loading: true, error: null });
      try {
        const { apps, deps, envs } = await firstValueFrom(
          forkJoin({
            apps: applicationService.getApplications(0, 5),
            deps: deploymentService.getDeployments(0, 5),
            envs: environmentService.getEnvironmentDashboard(),
          })
        );
        patchState(store, {
          applications: apps.items,
          recentDeployments: deps.items,
          environments: envs,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load dashboard data',
        });
      }
    },

    async promoteDeployment(deploymentId: string, targetEnvironmentId: string) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(deploymentService.promoteDeployment(deploymentId, { targetEnvironmentId }));
        const envs = await firstValueFrom(environmentService.getEnvironmentDashboard());
        patchState(store, { environments: envs, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.error?.message || error.message || 'Failed to promote deployment'
        });
      }
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
