import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApplicationService } from '../../core/services/application.service';
import { DeploymentService } from '../../core/services/deployment.service';
import { ApplicationDto, DeploymentDto, PaginatedResponse } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  applications: ApplicationDto[] = [];
  recentDeployments: DeploymentDto[] = [];
  loading = true;
  error = '';

  constructor(
    private applicationService: ApplicationService,
    private deploymentService: DeploymentService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  private loadDashboard(): void {
    this.loading = true;
    
    // Load applications
    this.applicationService.getApplications(0, 5).subscribe({
      next: (response: PaginatedResponse<ApplicationDto>) => {
        this.applications = response.items;
      },
      error: (err) => {
        this.error = 'Failed to load applications';
        console.error(err);
      }
    });

    // Load recent deployments
    this.deploymentService.getDeployments(0, 5).subscribe({
      next: (response: PaginatedResponse<DeploymentDto>) => {
        this.recentDeployments = response.items;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load deployments';
        console.error(err);
        this.loading = false;
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
