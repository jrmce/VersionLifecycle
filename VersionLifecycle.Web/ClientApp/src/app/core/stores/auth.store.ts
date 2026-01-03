import { Injectable, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LoginResponseDto, RegisterDto, RegisterWithNewTenantDto } from '../models/models';
import { UserRole } from '../enums';

interface AuthState {
  user: {
    userId: string;
    email: string;
    role: UserRole;
  } | null;
  token: string | null;
  refreshToken: string | null;
  tenantId: string | null;
  tenantCode: string | null;
  tenantName: string | null;
  status: 'idle' | 'loading' | 'authenticated' | 'error';
  error: string | null;
}

const storedToken = localStorage.getItem('accessToken');
const storedRefreshToken = localStorage.getItem('refreshToken');
const storedEmail = localStorage.getItem('email');
const storedRole = localStorage.getItem('role');
const storedUserId = localStorage.getItem('userId');
const storedTenantId = localStorage.getItem('tenantId');
const storedTenantName = localStorage.getItem('tenantName');
const storedTenantCode = localStorage.getItem('tenantCode');

const initialState: AuthState = {
  user: storedToken && storedEmail && storedRole
    ? {
        userId: storedUserId ?? '',
        email: storedEmail,
        role: storedRole as UserRole,
      }
    : null,
  token: storedToken,
  refreshToken: storedRefreshToken,
  tenantId: storedTenantId,
  tenantCode: storedTenantCode,
  tenantName: storedTenantName,
  status: storedToken ? 'authenticated' : 'idle',
  error: null,
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ status, token }) => ({
    isAuthenticated: computed(() => status() === 'authenticated' && !!token()),
    isLoading: computed(() => status() === 'loading'),
  })),
  withMethods((store, authService = inject(AuthService), router = inject(Router)) => ({
    async login(email: string, password: string, tenantId: string) {
      patchState(store, { status: 'loading', error: null });
      try {
        const response = await firstValueFrom(authService.login({ email, password, tenantId }));
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        if (response.role) localStorage.setItem('role', response.role);
        if (response.email) localStorage.setItem('email', response.email);
        if (response.userId) localStorage.setItem('userId', response.userId);
        if (response.tenantId) localStorage.setItem('tenantId', response.tenantId);
        if (response.tenantName) localStorage.setItem('tenantName', response.tenantName);
        if (response.tenantCode) localStorage.setItem('tenantCode', response.tenantCode);
        
        patchState(store, {
          token: response.accessToken,
          refreshToken: response.refreshToken,
          user: {
            userId: response.userId || '',
            email: response.email || email,
            role: (response.role || UserRole.Viewer) as UserRole,
          },
          tenantId: response.tenantId || tenantId,
          tenantName: response.tenantName || null,
          tenantCode: response.tenantCode || null,
          status: 'authenticated',
          error: null,
        });
        
        router.navigate(['/dashboard']);
      } catch (error: any) {
        patchState(store, {
          status: 'error',
          error: error.message || 'Login failed',
        });
      }
    },

    async register(data: RegisterDto) {
      patchState(store, { status: 'loading', error: null });
      try {
        const response = await firstValueFrom(authService.register(data));
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        if (response.role) localStorage.setItem('role', response.role);
        if (response.email) localStorage.setItem('email', response.email);
        if (response.userId) localStorage.setItem('userId', response.userId);
        if (response.tenantId) localStorage.setItem('tenantId', response.tenantId);
        if (response.tenantName) localStorage.setItem('tenantName', response.tenantName);
        if (response.tenantCode) localStorage.setItem('tenantCode', response.tenantCode);

        patchState(store, {
          token: response.accessToken,
          refreshToken: response.refreshToken,
          user: {
            userId: response.userId || '',
            email: response.email || data.email,
            role: (response.role || UserRole.Viewer) as UserRole,
          },
          tenantId: response.tenantId || data.tenantId,
          tenantCode: response.tenantCode || null,
          tenantName: response.tenantName || null,
          status: 'authenticated',
          error: null,
        });

        router.navigate(['/dashboard']);
      } catch (error: any) {
        patchState(store, {
          status: 'error',
          error: error.message || 'Registration failed',
        });
      }
    },

    async registerWithTenant(data: RegisterWithNewTenantDto) {
      patchState(store, { status: 'loading', error: null });
      try {
        const response = await firstValueFrom(authService.registerWithTenant(data));
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        if (response.role) localStorage.setItem('role', response.role);
        if (response.email) localStorage.setItem('email', response.email);
        if (response.userId) localStorage.setItem('userId', response.userId);
        if (response.tenantId) localStorage.setItem('tenantId', response.tenantId);
        if (response.tenantName) localStorage.setItem('tenantName', response.tenantName);
        if (response.tenantCode) localStorage.setItem('tenantCode', response.tenantCode);

        patchState(store, {
          token: response.accessToken,
          refreshToken: response.refreshToken,
          user: {
            userId: response.userId || '',
            email: response.email || data.email,
            role: (response.role || UserRole.Admin) as UserRole,
          },
          tenantId: response.tenantId || '',
          tenantCode: response.tenantCode || null,
          tenantName: response.tenantName || null,
          status: 'authenticated',
          error: null,
        });

        // Don't navigate immediately - let component show tenant code first
      } catch (error: any) {
        patchState(store, {
          status: 'error',
          error: error.message || 'Registration failed',
        });
      }
    },

    logout(): void {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('role');
      localStorage.removeItem('email');
      localStorage.removeItem('userId');
      localStorage.removeItem('tenantId');
      localStorage.removeItem('tenantName');
      localStorage.removeItem('tenantCode');
      
      patchState(store, {
        user: null,
        token: null,
        refreshToken: null,
        tenantId: null,
        tenantCode: null,
        tenantName: null,
        status: 'idle',
        error: null,
      });
      
      router.navigate(['/login']);
    },

    clearTenantInfo(): void {
      patchState(store, { tenantCode: null, tenantName: null });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
