import { Component, inject, OnInit } from '@angular/core';
import { UsersStore } from './users.store';
import { UsersListComponent } from './users-list.component';
import { AuthStore } from '../../core/stores/auth.store';
import { UserRole } from '../../core/enums';

/**
 * Container component for user list.
 * Manages state and orchestrates interactions with UsersStore.
 */
@Component({
  selector: 'app-users-list-container',
  standalone: true,
  imports: [UsersListComponent],
  template: `
    <app-users-list
      [users]="usersStore.users()"
      [loading]="usersStore.loading()"
      [error]="usersStore.error()"
      [isSuperAdmin]="isSuperAdmin"
      (editRole)="onEditRole($event)"
      (deleteUser)="onDeleteUser($event)"
      (clearError)="onClearError()"
    />
  `,
})
export class UsersListContainerComponent implements OnInit {
  protected readonly usersStore = inject(UsersStore);
  private readonly authStore = inject(AuthStore);

  get isSuperAdmin(): boolean {
    return this.authStore.user()?.role === UserRole.SuperAdmin;
  }

  ngOnInit() {
    this.usersStore.loadUsers();
  }

  onEditRole(event: { userId: string; role: string }) {
    this.usersStore.updateUserRole(event.userId, event.role);
  }

  onDeleteUser(userId: string) {
    this.usersStore.deleteUser(userId);
  }

  onClearError() {
    this.usersStore.clearError();
  }
}
