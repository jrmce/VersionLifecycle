import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationDto, VersionDto, EnvironmentDto, CreatePendingDeploymentDto } from '../../../core/models/models';
import { SelectInputComponent } from '../../../shared/components';
import type { SelectOption } from '../../../shared/components';

@Component({
  selector: 'app-deployments-timeline',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, SelectInputComponent],
  templateUrl: './deployments-timeline.component.html',
  styleUrls: ['./deployments-timeline.component.css']
})
export class DeploymentsTimelineComponent {
  @Input() applications: ApplicationDto[] = [];
  @Input() versions: VersionDto[] = [];
  @Input() environments: EnvironmentDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() success: string | null = null;

  @Output() applicationChange = new EventEmitter<string>();
  @Output() submitDeployment = new EventEmitter<CreatePendingDeploymentDto>();

  form: FormGroup;
  submitted = false;

  get applicationOptions(): SelectOption[] {
    return this.applications.map(app => ({
      label: app.name,
      value: app.id
    }));
  }

  get versionOptions(): SelectOption[] {
    return this.versions.map(version => ({
      label: `${version.versionNumber} (${version.status})`,
      value: version.id
    }));
  }

  get environmentOptions(): SelectOption[] {
    return this.environments.map(env => ({
      label: env.name,
      value: env.id
    }));
  }

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      applicationId: ['', Validators.required],
      versionId: ['', Validators.required],
      environmentId: ['', Validators.required]
    });
  }

  get f() {
    return this.form.controls;
  }

  onApplicationChange(): void {
    const appId = this.form.get('applicationId')?.value;
    if (appId) this.applicationChange.emit(appId);
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const deploymentData: CreatePendingDeploymentDto = {
      applicationId: this.form.get('applicationId')?.value,
      versionId: this.form.get('versionId')?.value,
      environmentId: this.form.get('environmentId')?.value
    };

    this.submitDeployment.emit(deploymentData);
  }
}
