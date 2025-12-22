import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationDto, PaginatedResponse } from '../../../core/models/models';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './applications-list.component.html',
  styleUrls: ['./applications-list.component.scss']
})
export class ApplicationsListComponent implements OnInit {
  applications: ApplicationDto[] = [];
  loading = true;
  error = '';
  currentPage = 0;
  pageSize = 10;
  totalCount = 0;

  constructor(private applicationService: ApplicationService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  private loadApplications(): void {
    this.loading = true;
    this.applicationService.getApplications(this.currentPage, this.pageSize).subscribe({
      next: (response: PaginatedResponse<ApplicationDto>) => {
        this.applications = response.items;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load applications';
        console.error(err);
        this.loading = false;
      }
    });
  }

  deleteApplication(id: number): void {
    if (confirm('Are you sure you want to delete this application?')) {
      this.applicationService.deleteApplication(id).subscribe({
        next: () => {
          this.applications = this.applications.filter(app => app.id !== id);
        },
        error: (err) => {
          this.error = 'Failed to delete application';
          console.error(err);
        }
      });
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  previousPage(): void {
    if (this.currentPage > 0) {
      this.currentPage--;
      this.loadApplications();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages - 1) {
      this.currentPage++;
      this.loadApplications();
    }
  }
}
