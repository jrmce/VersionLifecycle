import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { TenantLookupDto, TenantDto, CreateTenantDto, TenantStatsDto } from '../models/models';
import { TenantService } from '../services/tenant.service';
import { firstValueFrom } from 'rxjs';

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
    async loadTenants() {
      patchState(store, { loading: true, error: null });
      try {
        const tenants = await firstValueFrom(tenantService.getActiveTenants());
        patchState(store, { tenants, loading: false });
      } catch (err: any) {
        patchState(store, { loading: false, error: err?.message ?? 'Failed to load tenants' });
      }
    },

    async loadAdminTenants() {
      patchState(store, { loading: true, error: null });
      try {
        const adminTenants = await firstValueFrom(tenantService.getAllTenants());
        patchState(store, { adminTenants, loading: false });
      } catch (error: any) {
        patchState(store, { loading: false, error: error.message });
      }
    },

    async loadTenant(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        const selectedTenant = await firstValueFrom(tenantService.getTenant(id));
        patchState(store, { selectedTenant, loading: false });
      } catch (error: any) {
        patchState(store, { loading: false, error: error.message });
      }
    },

    async createTenant(dto: CreateTenantDto) {
      patchState(store, { loading: true, error: null });
      try {
        const newTenant = await firstValueFrom(tenantService.createTenant(dto));
        const adminTenants = [...store.adminTenants(), newTenant];
        patchState(store, { adminTenants, selectedTenant: newTenant, loading: false });
      } catch (error: any) {
        patchState(store, { loading: false, error: error.message });
      }
    },

    async updateTenant(id: string, dto: CreateTenantDto) {
      patchState(store, { loading: true, error: null });
      try {
        const updatedTenant = await firstValueFrom(tenantService.updateTenant(id, dto));
        const adminTenants = store.adminTenants().map((t) => (t.id === id ? updatedTenant : t));
        patchState(store, { adminTenants, selectedTenant: updatedTenant, loading: false });
      } catch (error: any) {
        patchState(store, { loading: false, error: error.message });
      }
    },

    async loadStats(id: string) {
      patchState(store, { loading: true, error: null });
      try {
        const stats = await firstValueFrom(tenantService.getTenantStats(id));
        patchState(store, { stats, loading: false });
      } catch (error: any) {
        patchState(store, { loading: false, error: error.message });
      }
    },

    clearSelectedTenant() {
      patchState(store, { selectedTenant: null });
    },
  }))
);

