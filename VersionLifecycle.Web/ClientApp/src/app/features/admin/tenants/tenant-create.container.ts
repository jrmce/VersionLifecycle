import { Component, effect, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';
import { TenantStore } from '../../../core/stores/tenant.store';
import { TenantCreateComponent } from './tenant-create.component';
import { CreateTenantDto } from '../../../core/models/models';

/**
 * Container component for creating a tenant.
 * Manages form state and orchestrates interactions with TenantStore.
 */
@Component({
  selector: 'app-tenant-create-container',
  standalone: true,
  imports: [TenantCreateComponent],
  template: `
    <app-tenant-create
      [form]="tenantForm"
      [loading]="tenantStore.loading()"
      [error]="tenantStore.error()"
      (submit)="onSubmit()"
      (cancel)="onCancel()"
    />
  `,
})
export class TenantCreateContainerComponent implements OnInit {
  protected readonly tenantStore = inject(TenantStore);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  tenantForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    subscriptionPlan: [''],
  });

  ngOnInit() {
    // Clear selectedTenant when entering create page to avoid stale state from edit/stats pages
    this.tenantStore.clearSelectedTenant();
  }

  private navigateOnSuccessEffect = effect(() => {
    if (!this.tenantStore.loading() && !this.tenantStore.error() && this.tenantStore.selectedTenant()) {
      this.router.navigate(['/admin/tenants']);
    }
  });

  onSubmit() {
    if (this.tenantStore.loading()) {
      return;
    }

    if (this.tenantForm.valid) {
      const dto: CreateTenantDto = {
        name: this.tenantForm.value.name!,
        description: this.tenantForm.value.description || undefined,
        subscriptionPlan: this.tenantForm.value.subscriptionPlan || undefined,
      };

      this.tenantStore.createTenant(dto);
    }
  }

  onCancel() {
    this.router.navigate(['/admin/tenants']);
  }
}
