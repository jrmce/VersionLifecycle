import { Component, inject, OnInit, effect } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';
import { TenantStore } from '../../../core/stores/tenant.store';
import { TenantEditComponent } from './tenant-edit.component';
import { CreateTenantDto } from '../../../core/models/models';

/**
 * Container component for editing a tenant.
 * Manages form state and orchestrates interactions with TenantStore.
 */
@Component({
  selector: 'app-tenant-edit-container',
  standalone: true,
  imports: [TenantEditComponent],
  template: `
    <app-tenant-edit
      [tenant]="tenantStore.selectedTenant()"
      [form]="tenantForm"
      [loading]="tenantStore.loading()"
      [error]="tenantStore.error()"
      (submit)="onSubmit()"
      (cancel)="onCancel()"
    />
  `,
})
export class TenantEditContainerComponent implements OnInit {
  protected readonly tenantStore = inject(TenantStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  tenantId: string | null = null;

  tenantForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    subscriptionPlan: [''],
  });

  // Effect to populate form when tenant is loaded
  private populateFormEffect = effect(() => {
    const tenant = this.tenantStore.selectedTenant();
    if (tenant && tenant.id === this.tenantId) {
      this.tenantForm.patchValue({
        name: tenant.name,
        description: tenant.subdomain, // Using subdomain as description for now
        subscriptionPlan: tenant.subscriptionPlan || '',
      });
    }
  });

  ngOnInit() {
    this.tenantId = this.route.snapshot.paramMap.get('id');
    if (this.tenantId) {
      this.tenantStore.loadTenant(this.tenantId);
    }
  }

  onSubmit() {
    if (this.tenantForm.valid && this.tenantId) {
      const dto: CreateTenantDto = {
        name: this.tenantForm.value.name!,
        description: this.tenantForm.value.description || undefined,
        subscriptionPlan: this.tenantForm.value.subscriptionPlan || undefined,
      };

      this.tenantStore.updateTenant(this.tenantId, dto);

      // Navigate back to list after successful update
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
