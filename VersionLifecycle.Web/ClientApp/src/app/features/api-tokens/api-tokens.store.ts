import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiTokenService } from '../../core/services/api-token.service';
import { ApiTokenDto, ApiTokenCreatedDto, CreateApiTokenDto, UpdateApiTokenDto } from '../../core/models/models';

interface ApiTokensState {
  tokens: ApiTokenDto[];
  selectedToken: ApiTokenDto | null;
  createdToken: ApiTokenCreatedDto | null; // Holds the newly created token with plaintext value
  loading: boolean;
  error: string | null;
}

const initialState: ApiTokensState = {
  tokens: [],
  selectedToken: null,
  createdToken: null,
  loading: false,
  error: null,
};

export const ApiTokensStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ tokens }) => ({
    hasTokens: computed(() => tokens().length > 0),
    activeTokens: computed(() => tokens().filter(t => t.isActive)),
    inactiveTokens: computed(() => tokens().filter(t => !t.isActive)),
  })),
  withMethods((store, apiTokenService = inject(ApiTokenService)) => ({
    async loadTokens() {
      patchState(store, { loading: true, error: null });
      try {
        const tokens = await firstValueFrom(apiTokenService.getApiTokens());
        patchState(store, {
          tokens,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load API tokens',
        });
      }
    },

    async loadToken(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        const token = await firstValueFrom(apiTokenService.getApiToken(id));
        patchState(store, {
          selectedToken: token,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load API token',
        });
      }
    },

    async createToken(dto: CreateApiTokenDto) {
      patchState(store, { loading: true, error: null, createdToken: null });
      try {
        const createdToken = await firstValueFrom(apiTokenService.createApiToken(dto));
        patchState(store, {
          tokens: [...store.tokens(), {
            id: createdToken.id,
            name: createdToken.name,
            description: createdToken.description,
            tokenPrefix: createdToken.tokenPrefix,
            expiresAt: createdToken.expiresAt,
            lastUsedAt: undefined,
            isActive: true,
            createdAt: createdToken.createdAt,
            createdBy: 'current-user',
          }],
          createdToken, // Store the full token with plaintext value
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to create API token',
        });
      }
    },

    async updateToken(id: number, dto: UpdateApiTokenDto) {
      patchState(store, { loading: true, error: null });
      try {
        const updatedToken = await firstValueFrom(apiTokenService.updateApiToken(id, dto));
        patchState(store, {
          tokens: store.tokens().map(t => t.id === id ? updatedToken : t),
          selectedToken: updatedToken,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to update API token',
        });
      }
    },

    async revokeToken(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(apiTokenService.revokeApiToken(id));
        patchState(store, {
          tokens: store.tokens().filter(t => t.id !== id),
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to revoke API token',
        });
      }
    },

    clearCreatedToken() {
      patchState(store, { createdToken: null });
    },

    clearError() {
      patchState(store, { error: null });
    },
  }))
);
