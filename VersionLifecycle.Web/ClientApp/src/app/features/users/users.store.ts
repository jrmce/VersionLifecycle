import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { UserService, UserDto } from '../../core/services/user.service';

interface UsersState {
  users: UserDto[];
  loading: boolean;
  error: string | null;
}

const initialState: UsersState = {
  users: [],
  loading: false,
  error: null,
};

export const UsersStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ users }) => ({
    hasUsers: computed(() => users().length > 0),
    adminUsers: computed(() => users().filter(u => u.role === 'Admin')),
    managerUsers: computed(() => users().filter(u => u.role === 'Manager')),
    viewerUsers: computed(() => users().filter(u => u.role === 'Viewer')),
  })),
  withMethods((store, userService = inject(UserService)) => ({
    async loadUsers() {
      patchState(store, { loading: true, error: null });
      try {
        const users = await firstValueFrom(userService.getTenantUsers());
        patchState(store, {
          users,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load users',
        });
      }
    },

    async updateUserRole(userId: string, role: string) {
      patchState(store, { loading: true, error: null });
      try {
        const updatedUser = await firstValueFrom(userService.updateUserRole(userId, role));
        patchState(store, {
          users: store.users().map(u => u.id === userId ? updatedUser : u),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to update user role',
        });
      }
    },

    async deleteUser(userId: string) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(userService.deleteUser(userId));
        patchState(store, {
          users: store.users().filter(u => u.id !== userId),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to delete user',
        });
      }
    },

    clearError() {
      patchState(store, { error: null });
    },
  }))
);
