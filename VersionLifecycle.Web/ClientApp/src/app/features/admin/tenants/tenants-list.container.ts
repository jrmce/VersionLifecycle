import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TenantStore } from '../../../core/stores/tenant.store';
import { TenantsListComponent } from './tenants-list.component';

/**
 * Container component for tenant list.
 * Manages state and orchestrates interactions with TenantStore.
 */
@Component({
  selector: 'app-tenants-list-container',
  standalone: true,
  imports: [TenantsListComponent],
  template: `
    <app-tenants-list
      [tenants]="tenantStore.adminTenants()"
      [loading]="tenantStore.loading()"
      [error]="tenantStore.error()"
      (create)="onCreateTenant()"
      (edit)="onEditTenant($event)"
      (viewStats)="onViewStats($event)"
      (searchChange)="onSearchChange($event)"
    />
  `,
})
export class TenantsListContainerComponent implements OnInit {
  protected readonly tenantStore = inject(TenantStore);
  private readonly router = inject(Router);

  ngOnInit() {
    this.tenantStore.loadAdminTenants();
  }

  onCreateTenant() {
    this.router.navigate(['/admin/tenants/create']);
  }

  onEditTenant(tenantId: string) {
    this.router.navigate(['/admin/tenants', tenantId, 'edit']);
  }

  onViewStats(tenantId: string) {
    this.router.navigate(['/admin/tenants', tenantId, 'stats']);
  }

  onSearchChange(searchTerm: string) {
    // Search is handled in presentational component for now
    // Could be moved to store if backend search is needed
  }
}
