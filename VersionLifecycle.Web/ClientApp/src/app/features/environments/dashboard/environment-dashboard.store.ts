import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { signalStore, withMethods, withState, patchState } from '@ngrx/signals';
import { EnvironmentDeploymentOverview } from '../../../core/models/models';
import { EnvironmentService } from '../../../core/services/environment.service';
import { DeploymentService } from '../../../core/services/deployment.service';

interface EnvironmentDashboardState {
  environments: EnvironmentDeploymentOverview[];
  loading: boolean;
  error: string | null;
}

const initialState: EnvironmentDashboardState = {
  environments: [],
  loading: false,
  error: null
};

export const EnvironmentDashboardStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, environmentService = inject(EnvironmentService), deploymentService = inject(DeploymentService)) => ({
    async loadDashboard() {
      patchState(store, { loading: true, error: null });
      try {
        const data = await firstValueFrom(environmentService.getEnvironmentDashboard());
        patchState(store, { environments: data, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.error?.message || error.message || 'Failed to load environment dashboard'
        });
      }
    },
    async promoteDeployment(deploymentId: number, targetEnvironmentId: number) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(deploymentService.promoteDeployment(deploymentId, { targetEnvironmentId }));
        const data = await firstValueFrom(environmentService.getEnvironmentDashboard());
        patchState(store, { environments: data, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.error?.message || error.message || 'Failed to promote deployment'
        });
      }
    },
    clearError() {
      patchState(store, { error: null });
    }
  }))
);
