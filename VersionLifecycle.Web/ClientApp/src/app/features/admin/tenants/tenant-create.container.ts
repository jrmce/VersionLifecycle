import { Component, inject } from '@angular/core';
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
export class TenantCreateContainerComponent {
  protected readonly tenantStore = inject(TenantStore);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  tenantForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    subscriptionPlan: [''],
  });

  onSubmit() {
    if (this.tenantForm.valid) {
      const dto: CreateTenantDto = {
        name: this.tenantForm.value.name!,
        description: this.tenantForm.value.description || undefined,
        subscriptionPlan: this.tenantForm.value.subscriptionPlan || undefined,
      };

      this.tenantStore.createTenant(dto);

      // Navigate back to list after successful creation
      // Note: In a real app, you'd want to wait for the observable to complete
      // and check for errors before navigating
      setTimeout(() => {
        if (!this.tenantStore.error()) {
          this.router.navigate(['/admin/tenants']);
        }
      }, 1000);
    }
  }

  onCancel() {
    this.router.navigate(['/admin/tenants']);
  }
}
