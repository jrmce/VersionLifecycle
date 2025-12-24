import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap, catchError, of } from 'rxjs';
import { TenantLookupDto, TenantDto, CreateTenantDto, TenantStatsDto } from '../models/models';
import { TenantService } from '../services/tenant.service';
import { HttpErrorResponse } from '@angular/common/http';

interface TenantState {
  tenants: TenantLookupDto[];
  adminTenants: TenantDto[];
  selectedTenant: TenantDto | null;
  stats: TenantStatsDto | null;
  loading: boolean;
  error: string | null;
}

const initialState: TenantState = {
  tenants: [],
  adminTenants: [],
  selectedTenant: null,
  stats: null,
  loading: false,
  error: null,
};

export const TenantStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, tenantService = inject(TenantService)) => ({
    loadTenants: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(() =>
          tenantService.getActiveTenants().pipe(
            tap((tenants) => patchState(store, { tenants, loading: false })),
            catchError((err) => {
              patchState(store, { loading: false, error: err?.message ?? 'Failed to load tenants' });
              return of([]);
            })
          )
        )
      )
    ),

    loadAdminTenants: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(() =>
          tenantService.getAllTenants().pipe(
            tap((adminTenants: TenantDto[]) => patchState(store, { adminTenants, loading: false })),
            catchError((error: HttpErrorResponse) => {
              patchState(store, { loading: false, error: error.message });
              return of([]);
            })
          )
        )
      )
    ),

    loadTenant: rxMethod<string>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          tenantService.getTenant(id).pipe(
            tap((selectedTenant: TenantDto) => patchState(store, { selectedTenant, loading: false })),
            catchError((error: HttpErrorResponse) => {
              patchState(store, { loading: false, error: error.message });
              return of(null);
            })
          )
        )
      )
    ),

    createTenant: rxMethod<CreateTenantDto>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((dto) =>
          tenantService.createTenant(dto).pipe(
            tap((newTenant: TenantDto) => {
              const adminTenants = [...store.adminTenants(), newTenant];
              patchState(store, { adminTenants, selectedTenant: newTenant, loading: false });
            }),
            catchError((error: HttpErrorResponse) => {
              patchState(store, { loading: false, error: error.message });
              return of(null);
            })
          )
        )
      )
    ),

    updateTenant: rxMethod<{ id: string; dto: CreateTenantDto }>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap(({ id, dto }) =>
          tenantService.updateTenant(id, dto).pipe(
            tap((updatedTenant: TenantDto) => {
              const adminTenants = store.adminTenants().map((t) => (t.id === id ? updatedTenant : t));
              patchState(store, { adminTenants, selectedTenant: updatedTenant, loading: false });
            }),
            catchError((error: HttpErrorResponse) => {
              patchState(store, { loading: false, error: error.message });
              return of(null);
            })
          )
        )
      )
    ),

    loadStats: rxMethod<string>(
      pipe(
        tap(() => patchState(store, { loading: true, error: null })),
        switchMap((id) =>
          tenantService.getTenantStats(id).pipe(
            tap((stats: TenantStatsDto) => patchState(store, { stats, loading: false })),
            catchError((error: HttpErrorResponse) => {
              patchState(store, { loading: false, error: error.message });
              return of(null);
            })
          )
        )
      )
    ),
  }))
);

