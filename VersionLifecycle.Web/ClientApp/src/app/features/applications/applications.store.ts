import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApplicationService } from '../../core/services/application.service';
import { ApplicationDto, CreateApplicationDto, UpdateApplicationDto } from '../../core/models/models';

interface ApplicationsState {
  applications: ApplicationDto[];
  selectedApplication: ApplicationDto | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  skip: number;
  take: number;
}

const initialState: ApplicationsState = {
  applications: [],
  selectedApplication: null,
  loading: false,
  error: null,
  totalCount: 0,
  skip: 0,
  take: 25,
};

export const ApplicationsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ applications, totalCount, skip, take }) => ({
    hasApplications: computed(() => applications().length > 0),
    currentPage: computed(() => Math.floor(skip() / take()) + 1),
    totalPages: computed(() => Math.ceil(totalCount() / take())),
    hasNextPage: computed(() => skip() + take() < totalCount()),
    hasPreviousPage: computed(() => skip() > 0),
  })),
  withMethods((store, applicationService = inject(ApplicationService)) => ({
    async loadApplications(skip = 0, take = 25) {
      patchState(store, { loading: true, error: null });
      try {
        const response = await firstValueFrom(applicationService.getApplications(skip, take));
        patchState(store, {
          applications: response.items,
          totalCount: response.totalCount,
          skip: response.skip,
          take: response.take,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load applications',
        });
      }
    },

    async loadApplication(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        const application = await firstValueFrom(applicationService.getApplication(id));
        patchState(store, {
          selectedApplication: application,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load application',
        });
      }
    },

    async createApplication(dto: CreateApplicationDto) {
      patchState(store, { loading: true, error: null });
      try {
        const newApplication = await firstValueFrom(applicationService.createApplication(dto));
        patchState(store, {
          applications: [...store.applications(), newApplication],
          totalCount: store.totalCount() + 1,
          selectedApplication: newApplication,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to create application',
        });
      }
    },

    async updateApplication(id: string, dto: UpdateApplicationDto) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(applicationService.updateApplication(id, dto));
        const updatedApplication = await firstValueFrom(applicationService.getApplication(id));
        const applications = store.applications().map((app) =>
          app.id === id ? updatedApplication : app
        );
        patchState(store, {
          applications,
          selectedApplication:
            store.selectedApplication()?.id === id
              ? updatedApplication
              : store.selectedApplication(),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to update application',
        });
      }
    },

    async deleteApplication(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(applicationService.deleteApplication(id));
        const applications = store.applications().filter((app) => app.id !== id);
        patchState(store, {
          applications,
          totalCount: store.totalCount() - 1,
          selectedApplication:
            store.selectedApplication()?.id === id ? null : store.selectedApplication(),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to delete application',
        });
      }
    },

    setSelectedApplication(application: ApplicationDto | null): void {
      patchState(store, { selectedApplication: application });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
