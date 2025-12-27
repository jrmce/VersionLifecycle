import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnvironmentDeploymentOverview } from '../../../core/models/models';

@Component({
  selector: 'app-environment-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './environment-dashboard.component.html'
})
export class EnvironmentDashboardComponent {
  @Input() environments: EnvironmentDeploymentOverview[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;

  @Output() reload = new EventEmitter<void>();
  @Output() clearError = new EventEmitter<void>();
  @Output() promote = new EventEmitter<{ deploymentId: number; targetEnvironmentId: number }>();

  nextEnvironment(currentOrder: number): EnvironmentDeploymentOverview | null {
    const higher = this.environments
      .filter(env => env.order > currentOrder)
      .sort((a, b) => a.order - b.order);
    return higher.length ? higher[0] : null;
  }

  isVersionPresentInEnv(env: EnvironmentDeploymentOverview, applicationId: number, versionId: number): boolean {
    return env.deployments.some(d => d.applicationId === applicationId && d.versionId === versionId);
  }
}
