import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DeploymentService } from '../../../core/services/deployment.service';
import { DeploymentDto, DeploymentEventDto } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './deployments-detail.component.html',
  styleUrls: ['./deployments-detail.component.scss']
})
export class DeploymentsDetailComponent implements OnInit {
  deployment: DeploymentDto | null = null;
  events: DeploymentEventDto[] = [];
  loading = true;
  error = '';
  success = '';
  deploymentId: number | null = null;

  constructor(
    private deploymentService: DeploymentService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.deploymentId = params['id'];
        this.loadDeployment();
      }
    });
  }

  private loadDeployment(): void {
    if (!this.deploymentId) return;

    this.deploymentService.getDeployment(this.deploymentId).subscribe({
      next: (deployment) => {
        this.deployment = deployment;
        this.loadEvents();
      },
      error: (err) => {
        this.error = 'Failed to load deployment';
        console.error(err);
        this.loading = false;
      }
    });
  }

  private loadEvents(): void {
    if (!this.deploymentId) return;

    this.deploymentService.getDeploymentEvents(this.deploymentId).subscribe({
      next: (events) => {
        this.events = events;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load events', err);
        this.loading = false;
      }
    });
  }

  confirmDeployment(): void {
    if (!this.deploymentId || this.deployment?.status !== 'Pending') {
      return;
    }

    if (!confirm('Are you sure you want to confirm this deployment?')) {
      return;
    }

    this.deploymentService.confirmDeployment(this.deploymentId).subscribe({
      next: () => {
        this.success = 'Deployment confirmed! Status updated to InProgress.';
        this.loadDeployment();
      },
      error: (err) => {
        this.error = err.message || 'Failed to confirm deployment';
      }
    });
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
}
