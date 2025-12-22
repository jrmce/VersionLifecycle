import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplicationService } from '../../../core/services/application.service';
import { VersionService } from '../../../core/services/version.service';
import { EnvironmentService } from '../../../core/services/environment.service';
import { ApplicationDto, VersionDto, EnvironmentDto } from '../../../core/models/models';

@Component({
  selector: 'app-applications-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './applications-detail.component.html',
  styleUrls: ['./applications-detail.component.scss']
})
export class ApplicationsDetailComponent implements OnInit {
  form: FormGroup;
  application: ApplicationDto | null = null;
  versions: VersionDto[] = [];
  environments: EnvironmentDto[] = [];
  loading = true;
  error = '';
  success = '';
  submitted = false;
  isNew = true;
  applicationId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private applicationService: ApplicationService,
    private versionService: VersionService,
    private environmentService: EnvironmentService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      repositoryUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]]
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id'] && params['id'] !== 'new') {
        this.applicationId = params['id'];
        this.isNew = false;
        this.loadApplication();
      } else {
        this.loading = false;
      }
    });
  }

  private loadApplication(): void {
    if (!this.applicationId) return;

    this.applicationService.getApplication(this.applicationId).subscribe({
      next: (app) => {
        this.application = app;
        this.form.patchValue(app);
        this.loadVersions();
        this.loadEnvironments();
      },
      error: (err) => {
        this.error = 'Failed to load application';
        console.error(err);
        this.loading = false;
      }
    });
  }

  private loadVersions(): void {
    if (!this.applicationId) return;

    this.versionService.getVersions(this.applicationId).subscribe({
      next: (versions) => {
        this.versions = versions;
      },
      error: (err) => {
        console.error('Failed to load versions', err);
      }
    });
  }

  private loadEnvironments(): void {
    if (!this.applicationId) return;

    this.environmentService.getEnvironments(this.applicationId).subscribe({
      next: (environments) => {
        this.environments = environments;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load environments', err);
        this.loading = false;
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

    if (this.isNew) {
      this.applicationService.createApplication(this.form.value).subscribe({
        next: (app) => {
          this.success = 'Application created successfully!';
          setTimeout(() => {
            this.router.navigate(['/applications', app.id]);
          }, 1500);
        },
        error: (err) => {
          this.error = err.message || 'Failed to create application';
        }
      });
    } else if (this.applicationId) {
      this.applicationService.updateApplication(this.applicationId, this.form.value).subscribe({
        next: () => {
          this.success = 'Application updated successfully!';
        },
        error: (err) => {
          this.error = err.message || 'Failed to update application';
        }
      });
    }
  }
}
