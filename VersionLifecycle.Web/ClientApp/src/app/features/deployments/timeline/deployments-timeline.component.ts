import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationDto, VersionDto, EnvironmentDto, CreatePendingDeploymentDto } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-timeline',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
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

  @Output() applicationChange = new EventEmitter<number>();
  @Output() submitDeployment = new EventEmitter<CreatePendingDeploymentDto>();

  form: FormGroup;
  submitted = false;

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
    const appId = Number(this.form.get('applicationId')?.value);
    if (appId) this.applicationChange.emit(appId);
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const deploymentData: CreatePendingDeploymentDto = {
      applicationId: Number(this.form.get('applicationId')?.value),
      versionId: Number(this.form.get('versionId')?.value),
      environmentId: Number(this.form.get('environmentId')?.value)
    };

    this.submitDeployment.emit(deploymentData);
  }
}
