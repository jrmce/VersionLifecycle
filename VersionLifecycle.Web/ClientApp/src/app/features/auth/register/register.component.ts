import { Component, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { RegisterDto, RegisterWithNewTenantDto } from '../../../core/models/models';
import { AuthStore } from '../../../core/stores/auth.store';
import { TenantStore } from '../../../core/stores/tenant.store';

type RegistrationMode = 'join' | 'create';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  mode: RegistrationMode = 'join';
  form: FormGroup;
  submitted = false;
  showSuccessModal = false;
  
  authStore = inject(AuthStore);
  tenantStore = inject(TenantStore);
  private router = inject(Router);
  
  private setDefaultTenant = effect(() => {
    const tenants = this.tenantStore.tenants();
    if (this.mode === 'join' && !this.form.value.tenantId && tenants.length > 0) {
      this.form.patchValue({ tenantId: tenants[0].id });
    }
  });

  constructor(
    private fb: FormBuilder,
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/)]],
      confirmPassword: ['', [Validators.required]],
      displayName: [''],
      // Join existing tenant fields
      tenantId: [''],
      tenantCode: [''],
      // Create new tenant fields
      tenantName: [''],
      tenantDescription: ['']
    });

    // Update validators when mode changes
    this.updateValidators();
  }

  ngOnInit(): void {
    this.tenantStore.loadTenants();
  }

  get f() {
    return this.form.controls;
  }

  switchMode(mode: RegistrationMode): void {
    this.mode = mode;
    this.submitted = false;
    this.form.reset();
    this.authStore.clearError();
    this.updateValidators();
  }

  private updateValidators(): void {
    const tenantIdControl = this.form.get('tenantId');
    const tenantCodeControl = this.form.get('tenantCode');
    const tenantNameControl = this.form.get('tenantName');

    if (this.mode === 'join') {
      tenantIdControl?.setValidators([Validators.required]);
      tenantCodeControl?.setValidators([Validators.required]);
      tenantNameControl?.clearValidators();
    } else {
      tenantIdControl?.clearValidators();
      tenantCodeControl?.clearValidators();
      tenantNameControl?.setValidators([Validators.required, Validators.maxLength(255)]);
    }

    tenantIdControl?.updateValueAndValidity();
    tenantCodeControl?.updateValueAndValidity();
    tenantNameControl?.updateValueAndValidity();
  }

  passwordsMatch(): boolean {
    const password = this.form.get('password')?.value;
    const confirmPassword = this.form.get('confirmPassword')?.value;
    return password === confirmPassword;
  }

  async onSubmit(): Promise<void> {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    if (!this.passwordsMatch()) {
      return;
    }

    if (this.mode === 'join') {
      const registerData: RegisterDto = {
        email: this.form.value.email,
        password: this.form.value.password,
        confirmPassword: this.form.value.confirmPassword,
        tenantId: this.form.value.tenantId,
        tenantCode: this.form.value.tenantCode,
        displayName: this.form.value.displayName || undefined
      };
      await this.authStore.register(registerData);
    } else {
      const registerData: RegisterWithNewTenantDto = {
        email: this.form.value.email,
        password: this.form.value.password,
        confirmPassword: this.form.value.confirmPassword,
        tenantName: this.form.value.tenantName,
        tenantDescription: this.form.value.tenantDescription || undefined,
        displayName: this.form.value.displayName || undefined
      };
      await this.authStore.registerWithTenant(registerData);
      
      // Show success modal with tenant code if registration was successful
      if (this.authStore.status() === 'authenticated' && this.authStore.tenantCode()) {
        this.showSuccessModal = true;
      }
    }
  }

  copyTenantCode(): void {
    const code = this.authStore.tenantCode();
    if (code) {
      navigator.clipboard.writeText(code);
    }
  }

  closeSuccessModal(): void {
    this.showSuccessModal = false;
    this.authStore.clearTenantInfo();
    this.router.navigate(['/dashboard']);
  }
}
