import { Injectable, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, tap, switchMap, catchError, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LoginResponseDto, RegisterDto } from '../models/models';

interface AuthState {
  user: {
    userId: string;
    email: string;
    role: string;
  } | null;
  token: string | null;
  refreshToken: string | null;
  tenantId: string | null;
  status: 'idle' | 'loading' | 'authenticated' | 'error';
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  token: localStorage.getItem('accessToken'),
  refreshToken: localStorage.getItem('refreshToken'),
  tenantId: null,
  status: localStorage.getItem('accessToken') ? 'authenticated' : 'idle',
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
    login: rxMethod<{ email: string; password: string; tenantId: string }>(
      pipe(
        tap(() => patchState(store, { status: 'loading', error: null })),
        switchMap(({ email, password, tenantId }) =>
          authService.login({ email, password, tenantId }).pipe(
            tap((response: LoginResponseDto) => {
              localStorage.setItem('accessToken', response.accessToken);
              localStorage.setItem('refreshToken', response.refreshToken);
              
              patchState(store, {
                token: response.accessToken,
                refreshToken: response.refreshToken,
                user: {
                  userId: response.userId || '',
                  email: response.email || email,
                  role: response.role || '',
                },
                tenantId: response.tenantId || tenantId,
                status: 'authenticated',
                error: null,
              });
              
              router.navigate(['/dashboard']);
            }),
            catchError((error) => {
              patchState(store, {
                status: 'error',
                error: error.message || 'Login failed',
              });
              return of(null);
            })
          )
        )
      )
    ),

    register: rxMethod<RegisterDto>(
      pipe(
        tap(() => patchState(store, { status: 'loading', error: null })),
        switchMap((data) =>
          authService.register(data).pipe(
            tap((response: LoginResponseDto) => {
              localStorage.setItem('accessToken', response.accessToken);
              localStorage.setItem('refreshToken', response.refreshToken);

              patchState(store, {
                token: response.accessToken,
                refreshToken: response.refreshToken,
                user: {
                  userId: response.userId || '',
                  email: response.email || data.email,
                  role: response.role || '',
                },
                tenantId: response.tenantId || data.tenantId,
                status: 'authenticated',
                error: null,
              });

              router.navigate(['/dashboard']);
            }),
            catchError((error) => {
              patchState(store, {
                status: 'error',
                error: error.message || 'Registration failed',
              });
              return of(null);
            })
          )
        )
      )
    ),

    logout(): void {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      
      patchState(store, {
        user: null,
        token: null,
        refreshToken: null,
        tenantId: null,
        status: 'idle',
        error: null,
      });
      
      router.navigate(['/login']);
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
