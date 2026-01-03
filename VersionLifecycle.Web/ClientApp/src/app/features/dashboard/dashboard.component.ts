import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardStore } from './dashboard.store';
import { HumanizeStatusPipe } from '../../core/pipes/humanize-status.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, HumanizeStatusPipe],
  providers: [DashboardStore],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  store = inject(DashboardStore);

  ngOnInit(): void {
    this.store.loadDashboard();
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Success': 'success',
      'Failed': 'danger',
      'Cancelled': 'secondary'
    };
    return colors[status] || 'secondary';
  }

  nextEnvironment(currentOrder: number) {
    const higher = this.store.environments()
      .filter(env => env.order > currentOrder)
      .sort((a, b) => a.order - b.order);
    return higher.length ? higher[0] : null;
  }

  isVersionPresentInEnv(env: any, applicationId: string, versionId: string): boolean {
    return env.deployments.some((d: any) => d.applicationId === applicationId && d.versionId === versionId);
  }

  onPromote(deploymentId: string, targetEnvironmentId: string): void {
    this.store.promoteDeployment(deploymentId, targetEnvironmentId);
  }
}
