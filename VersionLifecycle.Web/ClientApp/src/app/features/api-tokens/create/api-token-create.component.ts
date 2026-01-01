import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CreateApiTokenDto, ApiTokenCreatedDto } from '../../../core/models/models';

@Component({
  selector: 'app-api-token-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
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

  private resetForm(): void {
    this.tokenName = '';
    this.tokenDescription = '';
    this.expiresInDays = 90;
    this.tokenCopied = false;
  }
}
