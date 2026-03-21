import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InsightsStore } from './insights.store';

@Component({
  selector: 'app-insights',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './insights.component.html',
  styleUrls: ['./insights.component.css'],
})
export class InsightsComponent implements OnInit {
  store = inject(InsightsStore);
  question = '';

  @ViewChild('conversationEnd') conversationEnd!: ElementRef;

  ngOnInit(): void {
    this.store.checkAvailability();
  }

  askQuestion(): void {
    const trimmed = this.question.trim();
    if (!trimmed || this.store.loading()) return;

    this.store.askQuestion(trimmed);
    this.question = '';
    setTimeout(() => this.scrollToBottom(), 100);
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.askQuestion();
    }
  }

  clearConversations(): void {
    this.store.clearConversations();
  }

  private scrollToBottom(): void {
    this.conversationEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
  }

  readonly suggestedQuestions = [
    'What applications do I have?',
    'Which versions were deployed to production recently?',
    'What is the status of my latest deployments?',
    'How many environments are configured?',
  ];

  askSuggested(q: string): void {
    this.question = q;
    this.askQuestion();
  }
}
