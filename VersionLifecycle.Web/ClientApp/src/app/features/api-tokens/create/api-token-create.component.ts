import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CreateApiTokenDto, ApiTokenCreatedDto } from '../../../core/models/models';
import { SelectInputComponent } from '../../../shared/components';
import type { SelectOption } from '../../../shared/components';

@Component({
  selector: 'app-api-token-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SelectInputComponent],
  templateUrl: './api-token-create.component.html',
  styleUrls: ['./api-token-create.component.css']
})
export class ApiTokenCreateComponent {
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() createdToken: ApiTokenCreatedDto | null = null;

  @Output() create = new EventEmitter<CreateApiTokenDto>();
  @Output() clearToken = new EventEmitter<void>();

  tokenName = '';
  tokenDescription = '';
  expiresInDays: number | null = 90;
  tokenCopied = false;
  
  get expirationOptions(): SelectOption[] {
    return [
      { label: '30 days', value: 30 },
      { label: '90 days (recommended)', value: 90 },
      { label: '180 days', value: 180 },
      { label: '1 year', value: 365 },
      { label: 'Never (not recommended)', value: null }
    ];
  }

  onSubmit(): void {
    if (!this.tokenName.trim()) {
      return;
    }

    const dto: CreateApiTokenDto = {
      name: this.tokenName.trim(),
      description: this.tokenDescription.trim() || undefined,
      expiresAt: this.expiresInDays 
        ? new Date(Date.now() + this.expiresInDays * 24 * 60 * 60 * 1000)
        : undefined
    };

    this.create.emit(dto);
  }

  async copyToken(): Promise<void> {
    if (this.createdToken) {
      try {
        await navigator.clipboard.writeText(this.createdToken.token);
        this.tokenCopied = true;
        setTimeout(() => this.tokenCopied = false, 2000);
      } catch (err) {
        console.error('Failed to copy token:', err);
      }
    }
  }

  onClearToken(): void {
    this.clearToken.emit();
    this.resetForm();
  }
  
  onExpirationChange(value: any): void {
    if (value === '' || value === null || value === undefined) {
      this.expiresInDays = null;
      return;
    }

    const numericValue = typeof value === 'number' ? value : Number(value);
    this.expiresInDays = Number.isNaN(numericValue) ? null : numericValue;
  }

  private resetForm(): void {
    this.tokenName = '';
    this.tokenDescription = '';
    this.expiresInDays = 90;
    this.tokenCopied = false;
  }
}
