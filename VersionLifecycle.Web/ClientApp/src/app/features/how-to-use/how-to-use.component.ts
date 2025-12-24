import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ViewportScroller } from '@angular/common';

@Component({
  selector: 'app-how-to-use',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './how-to-use.component.html',
  styleUrls: ['./how-to-use.component.css']
})
export class HowToUseComponent {
  // Sections for table of contents
  sections = [
    { id: 'overview', title: 'Overview' },
    { id: 'workflow', title: 'Complete Workflow' },
    { id: 'applications', title: 'Managing Applications' },
    { id: 'versions', title: 'Version Tracking' },
    { id: 'environments', title: 'Environment Setup' },
    { id: 'deployments', title: 'Deployment Process' },
    { id: 'webhooks', title: 'Webhook Integration' },
    { id: 'examples', title: 'Real-Life Examples' }
  ];

  constructor(private viewportScroller: ViewportScroller) {}

  scrollToSection(sectionId: string): void {
    this.viewportScroller.scrollToAnchor(sectionId);
  }
}
