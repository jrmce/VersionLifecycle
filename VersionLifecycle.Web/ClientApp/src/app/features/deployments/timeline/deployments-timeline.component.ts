import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApplicationService } from '../../../core/services/application.service';
import { VersionService } from '../../../core/services/version.service';
import { EnvironmentService } from '../../../core/services/environment.service';
import { DeploymentService } from '../../../core/services/deployment.service';
import { ApplicationDto, VersionDto, EnvironmentDto, CreatePendingDeploymentDto } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-timeline',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './deployments-timeline.component.html',
  styleUrls: ['./deployments-timeline.component.scss']
})
export class DeploymentsTimelineComponent implements OnInit {
  form: FormGroup;
  applications: ApplicationDto[] = [];
  selectedApplication: ApplicationDto | null = null;
  versions: VersionDto[] = [];
  environments: EnvironmentDto[] = [];
  
  loading = true;
  submitted = false;
  error = '';
  success = '';

  constructor(
    private fb: FormBuilder,
    private applicationService: ApplicationService,
    private versionService: VersionService,
    private environmentService: EnvironmentService,
    private deploymentService: DeploymentService,
    private router: Router
  ) {
    this.form = this.fb.group({
      applicationId: ['', Validators.required],
      versionId: ['', Validators.required],
      environmentId: ['', Validators.required]
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.loadApplications();
  }

  private loadApplications(): void {
    this.applicationService.getApplications(0, 100).subscribe({
      next: (response) => {
        this.applications = response.items;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load applications';
        console.error(err);
        this.loading = false;
      }
    });
  }

  onApplicationChange(): void {
    const appId = this.form.get('applicationId')?.value;
    if (appId) {
      this.selectedApplication = this.applications.find(a => a.id === parseInt(appId));
      this.loadVersions(appId);
      this.loadEnvironments(appId);
    }
  }

  private loadVersions(applicationId: number): void {
    this.versionService.getVersions(applicationId).subscribe({
      next: (versions) => {
        this.versions = versions;
      },
      error: (err) => {
        console.error('Failed to load versions', err);
      }
    });
  }

  private loadEnvironments(applicationId: number): void {
    this.environmentService.getEnvironments(applicationId).subscribe({
      next: (environments) => {
        this.environments = environments;
      },
      error: (err) => {
        console.error('Failed to load environments', err);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.form.invalid) {
      return;
    }

    const deploymentData: CreatePendingDeploymentDto = {
      versionId: this.form.get('versionId')?.value,
      environmentId: this.form.get('environmentId')?.value
    };

    this.deploymentService.createPendingDeployment(deploymentData).subscribe({
      next: (deployment) => {
        this.success = 'Deployment created! Waiting for confirmation...';
        setTimeout(() => {
          this.router.navigate(['/deployments', deployment.id]);
        }, 1500);
      },
      error: (err) => {
        this.error = err.message || 'Failed to create deployment';
      }
    });
  }
}
