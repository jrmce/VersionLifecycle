import { Component, inject, effect } from '@angular/core';
import { Router } from '@angular/router';
import { EnvironmentsStore } from '../environments.store';
import { EnvironmentEditComponent } from './environment-edit.component';
import { CreateEnvironmentDto } from '../../../core/models/models';

@Component({
  selector: 'app-environment-edit-container',
  standalone: true,
  imports: [EnvironmentEditComponent],
  template: `
    <app-environment-edit
      [loading]="store.loading()"
      [error]="store.error()"
      (save)="onSave($event)"
      (cancel)="onCancel()"
      (clearError)="onClearError()"
    />
  `
})
export class EnvironmentEditContainerComponent {
  readonly store = inject(EnvironmentsStore);
  private readonly router = inject(Router);

  private navigationEffect = effect(() => {
    const selected = this.store.selectedEnvironment();
    if (selected && !this.store.loading() && !this.store.error()) {
      this.router.navigate(['/environments']);
    }
  });

  onSave(dto: CreateEnvironmentDto): void {
    this.store.createEnvironment(dto);
  }

  onCancel(): void {
    this.router.navigate(['/environments']);
  }

  onClearError(): void {
    this.store.clearError();
  }
}
