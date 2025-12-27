import { Component, OnInit, inject } from '@angular/core';
import { EnvironmentDashboardStore } from './environment-dashboard.store';
import { EnvironmentDashboardComponent } from './environment-dashboard.component';

@Component({
  selector: 'app-environment-dashboard-container',
  standalone: true,
  imports: [EnvironmentDashboardComponent],
  templateUrl: './environment-dashboard.container.html'
})
export class EnvironmentDashboardContainerComponent implements OnInit {
  readonly store = inject(EnvironmentDashboardStore);

  ngOnInit(): void {
    this.store.loadDashboard();
  }

  onReload(): void {
    this.store.loadDashboard();
  }

  onClearError(): void {
    this.store.clearError();
  }

  onPromote(event: { deploymentId: number; targetEnvironmentId: number }): void {
    this.store.promoteDeployment(event.deploymentId, event.targetEnvironmentId);
  }
}
