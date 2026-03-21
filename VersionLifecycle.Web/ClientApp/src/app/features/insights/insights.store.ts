import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { InsightsService } from '../../core/services/insights.service';
import { InsightsResponseDto } from '../../core/models/models';

export interface ConversationEntry {
  question: string;
  answer: string;
  generatedAt: Date;
}

interface InsightsState {
  conversations: ConversationEntry[];
  loading: boolean;
  error: string | null;
  available: boolean | null;
}

const initialState: InsightsState = {
  conversations: [],
  loading: false,
  error: null,
  available: null,
};

export const InsightsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, insightsService = inject(InsightsService)) => ({
    async askQuestion(question: string) {
      patchState(store, { loading: true, error: null });
      try {
        const response: InsightsResponseDto = await firstValueFrom(
          insightsService.ask({ question })
        );
        const entry: ConversationEntry = {
          question: response.question,
          answer: response.answer,
          generatedAt: response.generatedAt,
        };
        patchState(store, {
          conversations: [...store.conversations(), entry],
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.error?.message || error.message || 'Failed to get answer',
        });
      }
    },

    async checkAvailability() {
      try {
        const status = await firstValueFrom(insightsService.getStatus());
        patchState(store, { available: status.available });
      } catch {
        patchState(store, { available: false });
      }
    },

    clearConversations(): void {
      patchState(store, { conversations: [], error: null });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
