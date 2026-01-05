import { Component, inject, effect } from '@angular/core';
import { EnvironmentsStore } from '../environments.store';
import { EnvironmentsListComponent } from './environments-list.component';

@Component({
  selector: 'app-environments-list-container',
  standalone: true,
  imports: [EnvironmentsListComponent],
  template: `
    <app-environments-list
      [environments]="store.environments()"
      [loading]="store.loading()"
      [error]="store.error()"
      (createEnvironment)="onCreateEnvironment($event)"
      (updateEnvironment)="onUpdateEnvironment($event)"
      (deleteEnvironment)="onDeleteEnvironment($event)"
      (clearError)="onClearError()"
    />
  `
})
export class EnvironmentsListContainerComponent {
  readonly store = inject(EnvironmentsStore);

  private loadEffect = effect(() => {
    this.store.loadEnvironments();
  });

  onCreateEnvironment(dto: any): void {
    this.store.createEnvironment(dto);
  }

  onUpdateEnvironment(event: { id: string; dto: any }): void {
    this.store.updateEnvironment(event.id, event.dto);
  }

  onDeleteEnvironment(id: string): void {
    this.store.deleteEnvironment(id);
  }

  onClearError(): void {
    this.store.clearError();
  }
}
