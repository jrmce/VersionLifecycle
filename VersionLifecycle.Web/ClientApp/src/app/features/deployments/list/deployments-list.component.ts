import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DeploymentService } from '../../../core/services/deployment.service';
import { DeploymentDto, PaginatedResponse, DeploymentStatus } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './deployments-list.component.html',
  styleUrls: ['./deployments-list.component.scss']
})
export class DeploymentsListComponent implements OnInit {
  deployments: DeploymentDto[] = [];
  loading = true;
  error = '';
  selectedStatus: DeploymentStatus | '' = '';
  currentPage = 0;
  pageSize = 10;
  totalCount = 0;

  statuses: DeploymentStatus[] = ['Pending', 'InProgress', 'Success', 'Failed', 'Cancelled'];

  constructor(private deploymentService: DeploymentService) {}

  ngOnInit(): void {
    this.loadDeployments();
  }

  private loadDeployments(): void {
    this.loading = true;
    const status = this.selectedStatus as DeploymentStatus | undefined;
    
    this.deploymentService.getDeployments(this.currentPage, this.pageSize, status).subscribe({
      next: (response: PaginatedResponse<DeploymentDto>) => {
        this.deployments = response.items;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load deployments';
        console.error(err);
        this.loading = false;
      }
    });
  }

  onStatusChange(): void {
    this.currentPage = 0;
    this.loadDeployments();
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

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  previousPage(): void {
    if (this.currentPage > 0) {
      this.currentPage--;
      this.loadDeployments();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages - 1) {
      this.currentPage++;
      this.loadDeployments();
    }
  }
}
