import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, tap, switchMap, catchError, of } from 'rxjs';
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
    loadApplications: rxMethod<{ skip?: number; take?: number }>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(({ skip = 0, take = 25 }) =>
          applicationService.getApplications(skip, take).pipe(
            tap((response) => {
              patchState(store, {
                applications: response.items,
                totalCount: response.totalCount,
                skip: response.skip,
                take: response.take,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load applications',
              });
              return of(null);
            })
          )
        )
      )
    ),

    loadApplication: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          applicationService.getApplication(id).pipe(
            tap((application) => {
              patchState(store, {
                selectedApplication: application,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to load application',
              });
              return of(null);
            })
          )
        )
      )
    ),

    createApplication: rxMethod<CreateApplicationDto>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((dto) =>
          applicationService.createApplication(dto).pipe(
            tap((newApplication) => {
              patchState(store, {
                applications: [...store.applications(), newApplication],
                totalCount: store.totalCount() + 1,
                selectedApplication: newApplication,
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to create application',
              });
              return of(null);
            })
          )
        )
      )
    ),

    updateApplication: rxMethod<{ id: number; dto: UpdateApplicationDto }>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(({ id, dto }) =>
          applicationService.updateApplication(id, dto).pipe(
            switchMap(() => applicationService.getApplication(id)),
            tap((updatedApplication) => {
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
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to update application',
              });
              return of(null);
            })
          )
        )
      )
    ),

    deleteApplication: rxMethod<number>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          applicationService.deleteApplication(id).pipe(
            tap(() => {
              const applications = store.applications().filter((app) => app.id !== id);
              patchState(store, {
                applications,
                totalCount: store.totalCount() - 1,
                selectedApplication:
                  store.selectedApplication()?.id === id ? null : store.selectedApplication(),
                loading: false,
              });
            }),
            catchError((error) => {
              patchState(store, {
                loading: false,
                error: error.message || 'Failed to delete application',
              });
              return of(null);
            })
          )
        )
      )
    ),

    setSelectedApplication(application: ApplicationDto | null): void {
      patchState(store, { selectedApplication: application });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
