import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { DeploymentService } from '../../core/services/deployment.service';
import { VersionService } from '../../core/services/version.service';
import { EnvironmentService } from '../../core/services/environment.service';
import { DeploymentDto, DeploymentEventDto, DeploymentStatus, VersionDto, EnvironmentDto, CreatePendingDeploymentDto, UpdateDeploymentStatusDto } from '../../core/models/models';

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
    async loadDeployments(skip = 0, take = 25, status?: DeploymentStatus) {
      patchState(store, { loading: true, error: null });
      try {
        const response = await firstValueFrom(deploymentService.getDeployments(skip, take, status));
        patchState(store, {
          deployments: response.items,
          totalCount: response.totalCount,
          skip: response.skip,
          take: response.take,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load deployments',
        });
      }
    },

    async loadRecentDeployments(count = 5) {
      patchState(store, { loading: true, error: null });
      try {
        const response = await firstValueFrom(deploymentService.getDeployments(0, count));
        patchState(store, {
          recentDeployments: response.items,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load recent deployments',
        });
      }
    },

    async loadVersions(applicationId: number) {
      patchState(store, { loading: true, error: null });
      try {
        const versions = await firstValueFrom(versionService.getVersions(applicationId));
        patchState(store, { versions, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load versions',
        });
      }
    },

    async loadEnvironments() {
      patchState(store, { loading: true, error: null });
      try {
        const environments = await firstValueFrom(environmentService.getEnvironments());
        patchState(store, { environments, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load environments',
        });
      }
    },

    async createPendingDeployment(dto: CreatePendingDeploymentDto) {
      patchState(store, { loading: true, error: null });
      try {
        const deployment = await firstValueFrom(deploymentService.createPendingDeployment(dto));
        patchState(store, { selectedDeployment: deployment, loading: false });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to create deployment',
        });
      }
    },

    async loadDeployment(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        const deployment = await firstValueFrom(deploymentService.getDeployment(id));
        patchState(store, {
          selectedDeployment: deployment,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load deployment',
        });
      }
    },

    async loadDeploymentEvents(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        const events = await firstValueFrom(deploymentService.getDeploymentEvents(id));
        patchState(store, {
          events,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load deployment events',
        });
      }
    },

    async confirmDeployment(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(deploymentService.confirmDeployment(id));
        const deployment = await firstValueFrom(deploymentService.getDeployment(id));
        patchState(store, {
          selectedDeployment: deployment,
          deployments: store.deployments().map(d => d.id === deployment.id ? deployment : d),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to confirm deployment',
        });
      }
    },

    async updateDeploymentStatus(id: number, dto: UpdateDeploymentStatusDto) {
      patchState(store, { loading: true, error: null });
      try {
        const updated = await firstValueFrom(deploymentService.updateDeploymentStatus(id, dto));
        patchState(store, {
          selectedDeployment: store.selectedDeployment()?.id === updated.id ? updated : store.selectedDeployment(),
          deployments: store.deployments().map(d => d.id === updated.id ? updated : d),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to update deployment status',
        });
      }
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
