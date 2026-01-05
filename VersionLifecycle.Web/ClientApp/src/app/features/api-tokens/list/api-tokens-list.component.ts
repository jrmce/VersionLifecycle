import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiTokenDto } from '../../../core/models/models';
import { DataTableComponent } from '../../../shared/components';
import type { TableColumn, TableAction } from '../../../shared/components';

@Component({
  selector: 'app-api-tokens-list',
  standalone: true,
  imports: [CommonModule, RouterLink, DataTableComponent],
  templateUrl: './api-tokens-list.component.html',
  styleUrls: ['./api-tokens-list.component.css']
})
export class ApiTokensListComponent {
  @Input() tokens: ApiTokenDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;

  @Output() revoke = new EventEmitter<string>();
  @Output() toggleActive = new EventEmitter<{ id: string; isActive: boolean }>();

  get tableColumns(): TableColumn[] {
    return [
      { key: 'name', label: 'Name' },
      { key: 'tokenPrefix', label: 'Token Prefix' },
      { key: 'statusText', label: 'Status' },
      { key: 'expiresAtFormatted', label: 'Expires' },
      { key: 'lastUsedAtFormatted', label: 'Last Used' },
      { key: 'createdAtFormatted', label: 'Created' }
    ];
  }

  get tableActions(): TableAction[] {
    return [
      {
        label: 'Activate',
        callback: (row: ApiTokenDto) => this.onToggleActive(row),
        class: 'px-3 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: ApiTokenDto) => !row.isActive && !this.isExpired(row)
      },
      {
        label: 'Deactivate',
        callback: (row: ApiTokenDto) => this.onToggleActive(row),
        class: 'px-3 py-1 bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: ApiTokenDto) => row.isActive && !this.isExpired(row)
      },
      {
        label: 'Revoke',
        callback: (row: ApiTokenDto) => this.onRevoke(row),
        class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors font-medium cursor-pointer'
      }
    ];
  }

  get formattedTokens(): any[] {
    return this.tokens.map(token => ({
      ...token,
      tokenPrefix: `${token.tokenPrefix}...`,
      statusText: this.getStatusText(token),
      expiresAtFormatted: this.formatDate(token.expiresAt),
      lastUsedAtFormatted: this.formatDate(token.lastUsedAt),
      createdAtFormatted: this.formatDate(token.createdAt)
    }));
  }

  getStatusText(token: ApiTokenDto): string {
    if (this.isExpired(token)) {
      return '⏰ Expired';
    }
    if (token.isActive) {
      return '✓ Active';
    }
    return '⏸ Inactive';
  }

  onRevoke(token: ApiTokenDto): void {
    if (confirm(`Are you sure you want to revoke the token "${token.name}"? This action cannot be undone.`)) {
      this.revoke.emit(token.id);
    }
  }

  onToggleActive(token: ApiTokenDto): void {
    this.toggleActive.emit({ id: token.id, isActive: !token.isActive });
  }

  isExpired(token: ApiTokenDto): boolean {
    if (!token.expiresAt) return false;
    return new Date(token.expiresAt) < new Date();
  }

  formatDate(date: Date | undefined): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });
  }

  trackById(token: any): string {
    return token.id;
  }
}
