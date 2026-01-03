import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStore } from '../../../core/stores/auth.store';
import { TenantService } from '../../../core/services/tenant.service';
import { TenantLookupDto } from '../../../core/models/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private tenantService = inject(TenantService);
  authStore = inject(AuthStore);
  
  form: FormGroup;
  submitted = false;
  tenants: TenantLookupDto[] = [];
  showCustomTenantId = false;

  constructor() {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      tenantId: ['', Validators.required],
      customTenantId: ['']
    });
  }

  ngOnInit(): void {
    // Check if already authenticated and redirect
    if (this.authStore.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
    
    // Load available tenants for dropdown
    this.loadTenants();
  }

  loadTenants(): void {
    this.tenantService.getActiveTenants().subscribe({
      next: (tenants) => {
        this.tenants = tenants;
        // Set demo tenant as default if available
        const demoTenant = tenants.find(t => t.id === 'demo-tenant-001');
        if (demoTenant) {
          this.form.patchValue({ tenantId: demoTenant.id });
        }
      },
      error: (err) => {
        console.error('Failed to load tenants:', err);
      }
    });
  }

  toggleCustomTenantId(): void {
    this.showCustomTenantId = !this.showCustomTenantId;
    if (this.showCustomTenantId) {
      this.form.patchValue({ tenantId: '' });
      this.form.get('customTenantId')?.setValidators([Validators.required]);
    } else {
      this.form.patchValue({ customTenantId: '' });
      this.form.get('customTenantId')?.clearValidators();
    }
    this.form.get('customTenantId')?.updateValueAndValidity();
  }

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const { email, password, tenantId, customTenantId } = this.form.value;
    const finalTenantId = this.showCustomTenantId ? customTenantId : tenantId;
    this.authStore.login(email, password, finalTenantId);
  }
}
