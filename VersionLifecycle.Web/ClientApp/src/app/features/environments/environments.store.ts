import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { EnvironmentService } from '../../core/services/environment.service';
import { EnvironmentDto, CreateEnvironmentDto, UpdateEnvironmentDto } from '../../core/models/models';

interface EnvironmentsState {
  environments: EnvironmentDto[];
  selectedEnvironment: EnvironmentDto | null;
  loading: boolean;
  error: string | null;
}

const initialState: EnvironmentsState = {
  environments: [],
  selectedEnvironment: null,
  loading: false,
  error: null
};

export const EnvironmentsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, environmentService = inject(EnvironmentService)) => ({
    async loadEnvironments() {
      patchState(store, { loading: true, error: null });
      try {
        const environments = await firstValueFrom(environmentService.getEnvironments());
        const sorted = [...environments].sort((a, b) => a.order - b.order);
        patchState(store, { environments: sorted, loading: false });
      } catch (error: any) {
        patchState(store, {
          error: error.error?.message || error.message || 'Failed to load environments',
          loading: false
        });
      }
    },

    async createEnvironment(dto: CreateEnvironmentDto) {
      patchState(store, { loading: true, error: null });
      try {
        const environment = await firstValueFrom(environmentService.createEnvironment(dto));
        const updated = [...store.environments(), environment].sort((a, b) => a.order - b.order);
        patchState(store, {
          environments: updated,
          selectedEnvironment: environment,
          loading: false
        });
      } catch (error: any) {
        patchState(store, {
          error: error.error?.message || error.message || 'Failed to create environment',
          loading: false
        });
      }
    },

    async updateEnvironment(id: string, dto: UpdateEnvironmentDto) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(environmentService.updateEnvironment(id, dto));
        const environment = await firstValueFrom(environmentService.getEnvironment(id));
        const updated = store.environments().map(e =>
          e.id === environment.id ? environment : e
        ).sort((a, b) => a.order - b.order);
        patchState(store, {
          environments: updated,
          loading: false
        });
      } catch (error: any) {
        patchState(store, {
          error: error.error?.message || error.message || 'Failed to update environment',
          loading: false
        });
      }
    },

    async deleteEnvironment(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(environmentService.deleteEnvironment(id));
        const updated = store.environments().filter(e => e.id !== id);
        patchState(store, { environments: updated, loading: false });
      } catch (error: any) {
        patchState(store, {
          error: error.error?.message || error.message || 'Failed to delete environment',
          loading: false
        });
      }
    },

    clearError: () => patchState(store, { error: null }),

    setSelectedEnvironment: (environment: EnvironmentDto | null) =>
      patchState(store, { selectedEnvironment: environment })
  }))
);
