import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { DashboardStore } from './dashboard.store';
import { HumanizeStatusPipe } from '../../core/pipes/humanize-status.pipe';
import { DeploymentsTableComponent } from '../../shared/components';
import type { TableAction } from '../../shared/components';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, HumanizeStatusPipe, DeploymentsTableComponent],
  providers: [DashboardStore],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  store = inject(DashboardStore);
  private router = inject(Router);

  ngOnInit(): void {
    this.store.loadDashboard();
  }

  get recentDeploymentsActions(): TableAction[] {
    return [
      {
        label: 'View →',
        callback: (row: any) => this.router.navigate(['/deployments', row.id]),
        class: 'text-purple-600 hover:text-purple-700 font-medium cursor-pointer'
      }
    ];
  }

  get formattedRecentDeployments(): any[] {
    return this.store.recentDeployments().slice(0, 3);
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
