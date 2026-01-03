import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiTokenDto } from '../../../core/models/models';

@Component({
  selector: 'app-api-tokens-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './api-tokens-list.component.html',
  styleUrls: ['./api-tokens-list.component.css']
})
export class ApiTokensListComponent {
  @Input() tokens: ApiTokenDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;

  @Output() revoke = new EventEmitter<string>();
  @Output() toggleActive = new EventEmitter<{ id: string; isActive: boolean }>();

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
    return new Date(date).toLocaleString();
  }
}
