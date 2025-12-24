import { Component, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RegisterDto } from '../../../core/models/models';
import { AuthStore } from '../../../core/stores/auth.store';
import { TenantStore } from '../../../core/stores/tenant.store';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  form: FormGroup;
  submitted = false;
  // Use store signals in template instead of local state
  authStore = inject(AuthStore);
  tenantStore = inject(TenantStore);
  private setDefaultTenant = effect(() => {
    const tenants = this.tenantStore.tenants();
    if (!this.form.value.tenantId && tenants.length > 0) {
      this.form.patchValue({ tenantId: tenants[0].id });
    }
  });

  constructor(
    private fb: FormBuilder,
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      tenantId: ['', Validators.required],
      tenantCode: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.tenantStore.loadTenants();
  }

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }
    const registerData: RegisterDto = this.form.value;
    this.authStore.register(registerData);
  }
}
